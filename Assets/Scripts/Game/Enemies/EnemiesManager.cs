using ExitGames.Client.Photon;
using Nrjwolf.Tools.AttachAttributes;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class EnemiesManager : MonoBehaviour, IOnEventCallback {

    [field: Header("Autoattach properties")]
    [field: SerializeField, FindObjectOfType, ReadOnlyField] private LevelManager levelManager { get; set; }
    [field: SerializeField, ReadOnlyField] private List<Transform> allSpawnPoints { get; set; } = new List<Transform>();
    [field: SerializeField, ReadOnlyField] protected Transform playerBase { get; set; }
    public Transform PlayerBase { get { return playerBase; } }

    [field: SerializeField, ReadOnlyField] private Transform[] spawnPointsZone1 { get; set; }
    [field: SerializeField, ReadOnlyField] private Transform[] spawnPointsZone2 { get; set; }

    [field: SerializeField] private bool revalidateProperties { get; set; }

    public enum WaveStates {
        Stopped,
        InProgress,
        OnCooldown
    }

    [field: Header("Enemies settings")]
    [field: SerializeField] private Enemy[] enemyTypesToSpawn { get; set; }
    [field: SerializeField] private float enemyRespawnCooldown { get; set; } = 2f;
    private float enemyRespawnTimer { get; set; } = 0f;
    [field: SerializeField] private int maxEnemiesPerWave { get; set; } = 10;
    [field: SerializeField] private int increaseEnemiesPerWave { get; set; } = 4;

    [field: Header("Enemies pool")]
    [field: SerializeField, ReadOnlyField] private List<Enemy> allEnemiesDisabled { get; set; } = new List<Enemy>();

    [field: Header("Enemy Zones")]
    [field: SerializeField] private GameObject zone2Wall { get; set; }

    [field: Header("Debug")]
    [field: SerializeField, ReadOnlyField] private WaveStates waveState { get; set; } = WaveStates.Stopped;
    public WaveStates WaveState { get => waveState; }
    [field: SerializeField, ReadOnlyField] private int currentEnemiesSpawned { get; set; }

    private bool zone2Open { get; set; }

    void OnValidate() {
#if UNITY_EDITOR
        UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        bool isValidPrefabStage = prefabStage != null && prefabStage.stageHandle.IsValid();
        bool prefabConnected = PrefabUtility.GetPrefabInstanceStatus(this.gameObject) == PrefabInstanceStatus.Connected;
        if (!isValidPrefabStage/* && prefabConnected*/) {
            if (revalidateProperties)
                ValidateAssings();
        }
#endif
    }

    void ValidateAssings() {
        // Get all the spawnPoints in the scene in a simplified way.
        if (spawnPointsZone1 == null || spawnPointsZone1.Length == 0 || revalidateProperties) {
            spawnPointsZone1 = GameObject.FindGameObjectsWithTag("Respawn_1").Select(x => x.transform).ToArray();
        }
        if (spawnPointsZone2 == null || spawnPointsZone2.Length == 0 || revalidateProperties) {
            spawnPointsZone2 = GameObject.FindGameObjectsWithTag("Respawn_2").Select(x => x.transform).ToArray();
        }

        if (allSpawnPoints == null || allSpawnPoints.Count == 0 || revalidateProperties) {
            //allSpawnPoints = GameObject.FindGameObjectsWithTag("Respawn").Select(x => x.transform).ToList();
            allSpawnPoints = spawnPointsZone1.ToList();
        }

        if (playerBase == null || revalidateProperties) {
            playerBase = GameObject.FindGameObjectWithTag("PlayerBase").transform/*.GetChild(0)*/;
        }

        revalidateProperties = false;
    }

    // ---------------- Multiplayer ----------------

    public const int ENEMIES_SYNC_TYPE = 51, ENEMIES_SYNC_SPAWN_POS = 52;

    private List<Enemy> allEnemiesListIstantiated { get; set; } = new List<Enemy>();

    void Start() {
        PhotonNetwork.AddCallbackTarget(this);
    }

    void Update() {
        // For avoid that enemies children do anything when the game is ended.
        if (LevelManager.isEnded)
            gameObject.SetActive(false);

        if (waveState == WaveStates.InProgress) {
            if (currentEnemiesSpawned < maxEnemiesPerWave) {
                // Spawn enemy
                if (enemyRespawnTimer <= 0) {
                    GetEnemyFromPool();

                    enemyRespawnTimer = enemyRespawnCooldown;
                }
                else {
                    enemyRespawnTimer -= Time.deltaTime;
                }
            }
        }
        else if (waveState == WaveStates.OnCooldown) {
            enemyRespawnTimer = 0f;
        }
        else if (waveState == WaveStates.Stopped) {
            enemyRespawnTimer = 0f;
        }

        //Debug.Log($"Wave state: {waveState} -> timer: {enemyRespawnTimer}/{enemyRespawnCooldown}");
    }

    private void GetEnemyFromPool() {
        switch (levelManager.CurrentWave) {
            case 3:
                // ToDo: Add new enemy type to spawn and open the new spawn zone.
                if (!zone2Open) {
                    zone2Open = true;
                    allSpawnPoints.AddRange(spawnPointsZone2.ToList());
                    zone2Wall.SetActive(false);
                }
                break;
        }

        //currentEnemiesSpawned++;

        // if disabled enemies list is empty, instantiate a new enemy.
        if (allEnemiesDisabled.Count == 0) {

            int randomEnemyIndex = 0;
            if (zone2Open) {
                randomEnemyIndex = GenerateRandomNumber(0, enemyTypesToSpawn.Length);
            }

            // ---------- Multiplayer ------------
            if (PhotonNetwork.IsMasterClient) {
                PhotonNetwork.RaiseEvent(ENEMIES_SYNC_TYPE, randomEnemyIndex, new RaiseEventOptions { Receivers = ReceiverGroup.All }, 
                    SendOptions.SendReliable);
            }

            //MP_EnemyInstantiation(randomEnemyIndex);
        }
        else {
            // Get the first enemy in the list and move it to a random spawn point.
            Enemy enemy = allEnemiesDisabled[0];
            allEnemiesDisabled.RemoveAt(0);
            enemy.Agent.enabled = false;
            if (PhotonNetwork.IsMasterClient) {
                MoveEnemyToRandomSpawn(enemy);
            }
            enemy.gameObject.SetActive(true);
            currentEnemiesSpawned++;
        }

    }

    public void MoveEnemyToRandomSpawn(Enemy enemy) {
        //int randomSpawnIndex = Random.Range(0, allSpawnPoints.Count);
        int randomSpawnIndex = GenerateRandomNumber(0, allSpawnPoints.Count);

        Vector3 spawnPos = NavMesh.SamplePosition(allSpawnPoints[randomSpawnIndex].position, out NavMeshHit hit,
            1f, NavMesh.AllAreas) ? hit.position : allSpawnPoints[randomSpawnIndex].position;

        if (randomSpawnIndex > 1)
            Debug.Log($"Enemy {enemy.name} has indexSpawn: {randomSpawnIndex}");

        /*currentEnemyTransform = null;
        currentEnemyTransform = enemy;*/

        // ---------- Multiplayer ------------
        //if (PhotonNetwork.IsMasterClient) {
            object[] data = new object[] { enemy.multiplayerID.ID, spawnPos };
            PhotonNetwork.RaiseEvent(ENEMIES_SYNC_SPAWN_POS, data, new RaiseEventOptions { Receivers = ReceiverGroup.All }, SendOptions.SendReliable);
        //}
        //MP_MoveEnemy(enemy, spawnPos);
    }

    public void StartEnemyRespawn() {
        if (levelManager.CurrentWave > 1) {
            maxEnemiesPerWave += increaseEnemiesPerWave;
        }
        waveState = WaveStates.InProgress;
    }

    public void StopEnemyRespawn() {
        waveState = WaveStates.OnCooldown;
    }

    public void StopWaves() {
        waveState = WaveStates.Stopped;
    }

    int GenerateRandomNumber(int minInclusive, int maxExclusive) {
        int seed = System.DateTime.Now.Millisecond + Time.frameCount;
        Random.InitState(seed);
        return Random.Range(minInclusive, maxExclusive);
    }

    public void AddEnemyToPool(Enemy enemy) {
        allEnemiesDisabled.Add(enemy);
        currentEnemiesSpawned--;
    }

    // ---------------- Multiplayer-Adapted Methods ----------------

    void MP_EnemyInstantiation(int randomEnemyIndex) {
        Enemy enemy = Instantiate(enemyTypesToSpawn[randomEnemyIndex], transform);
        //Enemy enemy = enemyGO.GetComponent<Enemy>();
        enemy.enemies = this;

        enemy.multiplayerID.ID = allEnemiesListIstantiated.Count;
        allEnemiesListIstantiated.Add(enemy);

        if (PhotonNetwork.IsMasterClient)
            MoveEnemyToRandomSpawn(enemy);

        currentEnemiesSpawned++;
    }

    void MP_MoveEnemy(Transform enemy, Vector3 spawnPos) {
        enemy.position = new Vector3(spawnPos.x, enemy.position.y, spawnPos.z);
    }

    void MP_MoveEnemyPatch(int enemyID, Vector3 spawnPos) {
        //currentEnemyTransform.position = new Vector3(spawnPos.x, currentEnemyTransform.position.y, spawnPos.z);

        Enemy enemy = allEnemiesListIstantiated[enemyID];
        enemy.Rb.isKinematic = true;
        enemy.Agent.enabled = false;
        enemy.transform.position = new Vector3(spawnPos.x, 0f, spawnPos.z);

        if (enemy.enemyType == Enemy.EnemyType.Runner)
            enemy.Rb.isKinematic = true;
        else
            enemy.Rb.isKinematic = false;

        enemy.Agent.enabled = true;

        //Debug.Log($"Enemy {allEnemiesListIstantiated[enemyID].name} moved to {spawnPos}");
    }

    public void OnEvent(EventData photonEvent) {
        // Sync random type selected for instantiation.
        if (photonEvent.Code == ENEMIES_SYNC_TYPE) {
            MP_EnemyInstantiation((int) photonEvent.CustomData);
        }

        // Sync enemy spawn position.
        if (photonEvent.Code == ENEMIES_SYNC_SPAWN_POS) {
            //object[] data = (object[]) photonEvent.CustomData;
            //MP_MoveEnemy((Transform)data[0], (Vector3)data[1]);

            //MP_MoveEnemyPatch((Vector3) photonEvent.CustomData);
            //currentEnemyTransform = null;   // Generate a null exception. Why? Maybe because reseting after currentEnemyTransform.position is setted again.
            // Ahora comentado, algunos se mueven muy seguidamente.

            object[] data = (object[]) photonEvent.CustomData;
            MP_MoveEnemyPatch((int)data[0], (Vector3)data[1]);
        }
    }
}