using Nrjwolf.Tools.AttachAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour {

	[field: Header("Autottach on Editor properties")]
    [field: SerializeField, GetComponent, ReadOnlyField] private CharacterController charJoke { get; set; }
    [field: SerializeField, ReadOnlyField] private Transform[] playerBaseSpawns { get; set; }
    [field: SerializeField, ReadOnlyField] private Image deadFadePanel { get; set; }
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
        if (deadFadePanel == null || revalidateProperties) {
            deadFadePanel = GameObject.Find("DeadFadePanel").GetComponent<Image>();
        }

        if (playerBaseSpawns == null || playerBaseSpawns.Length == 0 || revalidateProperties) {
            playerBaseSpawns = GameObject.FindGameObjectsWithTag("Respawn").Select(x => x.transform).ToArray();
        }

        revalidateProperties = false;
    }

    void Start() {
        centerWidth = Screen.width / 2;
        centerHeight = Screen.height / 2;

        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    void Update() {
        if (!isDead) {
            CheckEnemyInFront();
        }

        // Fix the ******* bug when CharacterJokeController collides with rigidbodies.
        //transform.position = new Vector3(transform.position.x, 0, transform.position.z);
    }

    public void TakeDamage(float damage) {
        if (currentHealth > 0) {
            currentHealth -= damage;
            if (currentHealth <= 0) {
                Die();
            }
            UpdateHealthUI();
        }
    }

    void Die() {
        isDead = true;
        StartCoroutine(DeathTeleportCo());
    }

    public void ChangeMoney(int amount) {
        money += amount;
        UpdateMoneyUI();
    }

    void UpdateHealthUI() {
        healthBar.fillAmount = currentHealth / maxHealth;
        healthText.text = $"{(int)currentHealth}/{maxHealth}";
    }

    void UpdateMoneyUI() {
        if (money > 9999999) {
            money = 9999999;
        }
        moneyText.text = $"$ {money}";
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
        if (other.CompareTag("Spear")) {
            TakeDamage(10);
        }
    }

    private void OnTriggerStay(Collider other) {
        if (other.CompareTag("Spear")) {
            TakeDamage(2f * Time.deltaTime);
        }
    }

    IEnumerator DeathTeleportCo() {
        // Fade in
        float t = 0;
        while (t < 1) {
            t += Time.deltaTime;
            deadFadePanel.color = new Color(0, 0, 0, t);
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
            deadFadePanel.color = new Color(0, 0, 0, t);
            yield return null;
        }

        isDead = false;
    }

    private void OnDestroy() {
        if (gameObject == null)
            return;

        StopAllCoroutines();
    }
}