using Cinemachine;
using Nrjwolf.Tools.AttachAttributes;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PhotonView))]
public class PlayerManager : MonoBehaviourPun, IPunObservable {

	[field: Header("Autottach on Editor properties")]
    [field: SerializeField, GetComponent, ReadOnlyField] private CharacterController charJoke { get; set; }
    [field: SerializeField, GetComponent, ReadOnlyField] public MeshRenderer render { get; set; }
    [field: SerializeField, GetComponent, ReadOnlyField] private TrapPlacer trapPlacer { get; set; }
    [field: SerializeField, ReadOnlyField] private Transform[] playerBaseSpawns { get; set; }
    [field: SerializeField, ReadOnlyField] private Image deadFadePanel { get; set; }
    [field: SerializeField, ReadOnlyField] private Transform camPivot { get; set; }
    [field: SerializeField] private bool revalidateProperties { get; set; }

    [field: Header("Player Settings")]
	[field: SerializeField] private float maxHealth { get; set; } = 100;
	[field: SerializeField] private float currentHealth { get; set; } = 100;
	[field: SerializeField] private int money { get; set; } = 0;
    public int Money { get { return money; } }

    [field: Header("UI Settings")]
    [field: SerializeField] private Image healthBar { get; set; }
    [field: SerializeField] private Text healthText { get; set; }
    [field: SerializeField] private Text moneyText { get; set; }
    [field: SerializeField] private float enemyRayCastDistance { get; set; } = 60f;
    [field: SerializeField] private LayerMask enemyLayer { get; set; } = 1 << 11;

    [field: Header("Debug")]
    [field: SerializeField, ReadOnlyField] private bool isDead { get; set; }
    public bool IsDead { get { return isDead; } }
    

    // Definir un evento que se dispara cuando el jugador mira a un enemigo
    public static event Action<Transform> OnPlayerLookAtEnemy;

    private float centerWidth { get; set; }
    private float centerHeight { get; set; }

    // -------------- Multiplayer --------------

    private Vector3 networkPos;
    private Quaternion networkRot;


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
        if (deadFadePanel == null || revalidateProperties) {
            deadFadePanel = GameObject.Find("DeadFadePanel").GetComponent<Image>();
        }

        if (playerBaseSpawns == null || playerBaseSpawns.Length == 0 || revalidateProperties) {
            playerBaseSpawns = GameObject.FindGameObjectsWithTag("Respawn").Select(x => x.transform).ToArray();
        }

        if (camPivot == null || revalidateProperties) {
            camPivot = transform.GetChild(1);
        }

        revalidateProperties = false;
    }

    void Start() {
        gameObject.name = $"_Player_ - {photonView.Owner.NickName}";

        centerWidth = Screen.width / 2;
        centerHeight = Screen.height / 2;

        currentHealth = maxHealth;

        StartCoroutine(LateComponentsCo());

        if (!photonView.IsMine) {
            gameObject.layer = 10;  // RemotePlayer
        }

        switch (photonView.ViewID) {
            case 2001:
                render.material.color = Color.green;
                break;
            case 3001:
                render.material.color = Color.blue;
                break;
        }
    }

    void Update() {
        if (photonView.IsMine) {
            if (!isDead) {
                CheckEnemyInFront();
            }

            if (Input.GetKeyDown(KeyCode.Escape)) {
                Cursor.visible = !Cursor.visible;
                Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
            }
        }
        /*else {
            SyncOtherPlayers();
        }*/

        // Fix the ******* bug when CharacterJokeController collides with rigidbodies. May produce camera rotation bug.
        //transform.position = new Vector3(transform.position.x, 0, transform.position.z);
    }

    void SyncOtherPlayers() {
        transform.position = Vector3.MoveTowards(transform.position, networkPos, Time.deltaTime * 1000f);

        if (networkRot != null)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, networkRot, Time.deltaTime * 1000f);
    }

    public void TakeDamage(float damage) {
        if (currentHealth > 0) {
            currentHealth -= damage;
            if (currentHealth <= 0) {
                photonView.RPC(nameof(RPC_Die), RpcTarget.All);
                //RPC_Die();
            }
            UpdateHealthUI();
        }
    }

    [PunRPC]
    void RPC_Die() {
        isDead = true;
        StartCoroutine(DeathTeleportCo());
    }

    public void ChangeMoney(int amount) {
        money += amount;
        UpdateMoneyUI();
    }

    void UpdateHealthUI() {
        if (photonView.IsMine) {
            healthBar.fillAmount = currentHealth / maxHealth;
            healthText.text = $"{(int)currentHealth} / {maxHealth}";
        }
    }

    void UpdateMoneyUI() {
        if (photonView.IsMine) {
            if (money > 9999999) {
                money = 9999999;
            }
            moneyText.text = $"$ {money}";
        }
    }

    void CheckEnemyInFront() {
        // Crear un rayo desde el centro de la cámara
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(centerWidth, centerHeight, 0));

        // Crear una variable para almacenar la información del raycast
        RaycastHit hit;

        // Realizar el raycast
        if (Physics.Raycast(ray, out hit, enemyRayCastDistance, enemyLayer)) {
            // Si el raycast golpea a un enemigo, disparar el evento
            OnPlayerLookAtEnemy?.Invoke(hit.transform);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (!photonView.IsMine)
            return;

        if (other.CompareTag("Spear")) {
            TakeDamage(10);
        }
    }

    private void OnTriggerStay(Collider other) {
        if (!photonView.IsMine)
            return;

        if (other.CompareTag("Spear")) {
            TakeDamage(2f * Time.deltaTime);
        }
    }

    IEnumerator DeathTeleportCo() {
        // Fade in
        float t = 0;
        Color renderColor = render.material.color;
        while (t < 1) {
            t += Time.deltaTime;
            if (photonView.IsMine)
                deadFadePanel.color = new Color(0, 0, 0, t);
            render.material.color = new Color(renderColor.r, renderColor.g, renderColor.b, 1 - t);
            yield return null;
        }

        yield return new WaitForSeconds(2f);

        charJoke.enabled = false;

        transform.position = playerBaseSpawns[0].position;
        transform.rotation = playerBaseSpawns[0].rotation;

        charJoke.enabled = true;

        currentHealth = maxHealth;
        UpdateHealthUI();

        // Fade out
        t = 1;
        while (t > 0) {
            t -= Time.deltaTime;
            render.material.color = new Color(renderColor.r, renderColor.g, renderColor.b, 1 - t);
            if (photonView.IsMine)
                deadFadePanel.color = new Color(0, 0, 0, t);
            yield return null;
        }

        isDead = false;
    }

    /*void OnDrawGizmos() {
#if UNITY_EDITOR
        GUI.color = Color.green;
        GUI.skin.label.fontSize = 40;
        Handles.Label(transform.position + new Vector3(-1.2f, 1.25f, 0), gameObject.name);
#endif
    }*/

    private void OnDestroy() {
        if (gameObject == null)
            return;

        StopAllCoroutines();
        if (photonView.IsMine) {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        /*if (stream.IsWriting) {
            // Enviamos la posición y la rotación del jugador.
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else {
            // Recibimos la posición y la rotación del jugador.
            networkPos = (Vector3)stream.ReceiveNext();
            networkRot = (Quaternion)stream.ReceiveNext();
        }*/
    }

    IEnumerator LateComponentsCo() {
        // All compontents that can't be attached on editor because now Player is spawned as Prefab.

        yield return new WaitForSeconds(0.5f);

        if (playerBaseSpawns == null || playerBaseSpawns.Length == 0) {
            playerBaseSpawns = GameObject.FindGameObjectsWithTag("Respawn").Select(x => x.transform).ToArray();
        }
        yield return null;

        if (photonView.IsMine) {
            if (deadFadePanel == null) {
                deadFadePanel = GameObject.Find("DeadFadePanel").GetComponent<Image>();
            }
            yield return null;
            if (healthBar == null) {
                healthBar = GameObject.Find("PlayerHealthBar").GetComponent<Image>();
                healthText = healthBar.transform.GetChild(0).GetComponent<Text>();
            }
            yield return null;
            if (moneyText == null) {
                moneyText = GameObject.Find("MoneyText").GetComponent<Text>();
            }
            yield return null;
            FindObjectOfType<CinemachineVirtualCamera>().Follow = camPivot;
            yield return null;
            if (trapPlacer.cam == null)
                trapPlacer.cam = Camera.main;
            if (trapPlacer.moneyCostText == null)
                trapPlacer.moneyCostText = GameObject.Find("MoneyTrapCostText").GetComponent<Text>();

            UpdateHealthUI();
        }
    }
}