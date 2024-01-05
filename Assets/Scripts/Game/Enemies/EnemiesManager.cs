using Nrjwolf.Tools.AttachAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class EnemiesManager : MonoBehaviour {

    [field: Header("Autoattach properties")]
    [field: SerializeField, FindObjectOfType, ReadOnlyField] private LevelManager levelManager { get; set; }
    [field: SerializeField, ReadOnlyField] private List<Transform> allSpawnPoints { get; set; } = new List<Transform>();
    [field: SerializeField] private bool revalidateProperties { get; set; }

    public enum WaveStates {
        Stopped,
        InProgress,
        OnCooldown
    }

    [field: Header("Enemies settings")]
    [field: SerializeField] private GameObject[] enemyTypesToSpawn { get; set; }
    [field: SerializeField] private float enemyRespawnCooldown { get; set; } = 2f;
    private float enemyRespawnTimer { get; set; } = 0f;
    [field: SerializeField] private int maxEnemiesPerWave { get; set; } = 10;
    [field: SerializeField] private int increaseEnemiesPerWave { get; set; } = 4;
    [field: SerializeField, ReadOnlyField] private List<Enemy> allEnemiesDisabled { get; set; } = new List<Enemy>();

    [field: Header("Debug")]
    [field: SerializeField, ReadOnlyField] private WaveStates waveState { get; set; } = WaveStates.Stopped;
    public WaveStates WaveState { get => waveState; }

    void OnValidate() {
#if UNITY_EDITOR
        UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        bool isValidPrefabStage = prefabStage != null && prefabStage.stageHandle.IsValid();
        bool prefabConnected = PrefabUtility.GetPrefabInstanceStatus(this.gameObject) == PrefabInstanceStatus.Connected;
        if (!isValidPrefabStage && prefabConnected) {
            if (revalidateProperties)
                ValidateAssings();
        }
#endif
    }

    void ValidateAssings() {
        // Get all the spawnPoints in the scene in a simplified way.

        if (allSpawnPoints == null || allSpawnPoints.Count == 0 || revalidateProperties) {
            //revalidateProperties = false;
            allSpawnPoints = GameObject.FindGameObjectsWithTag("Respawn").Select(x => x.transform).ToList();
        }
        revalidateProperties = false;
    }

    /*private bool playerDetected { get; set; }
    public bool PlayerDetected { get => playerDetected; set => playerDetected = value; }*/

    void Update() {
        if (waveState == WaveStates.InProgress) {
            // Spawn enemies
            if (allEnemiesDisabled.Count > 0) {
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
                // ToDo: Add new enemy type to spawn.
                break;
        }
    }

    public void MoveEnemyToRandomSpawn(Transform enemy) {
        //int randomSpawnIndex = Random.Range(0, allSpawnPoints.Count);
        int randomSpawnIndex = GenerateRandomNumber(0, allSpawnPoints.Count);

        enemy.position = NavMesh.SamplePosition(allSpawnPoints[randomSpawnIndex].position, out NavMeshHit hit,
            1f, NavMesh.AllAreas) ? hit.position : allSpawnPoints[randomSpawnIndex].position;
    }

    public void StartEnemyRespawn() {
        waveState = WaveStates.InProgress;
    }

    public void StopEnemyRespawn() {
        waveState = WaveStates.OnCooldown;
    }

    public void StopWaves() {
        waveState = WaveStates.Stopped;
    }

    int GenerateRandomNumber(int minInclusive, int maxExclusive) {
        int seed = System.DateTime.Now.Millisecond;
        Random.InitState(seed);
        return Random.Range(minInclusive, maxExclusive);
    }
}