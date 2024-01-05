using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour {

	//[field: Header("Autottach on Editor properties")]

	[field: Header("Player Settings")]
	[field: SerializeField] private int maxHealth { get; set; } = 100;
	[field: SerializeField] private int currentHealth { get; set; } = 100;
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

    void Start() {
        centerWidth = Screen.width / 2;
        centerHeight = Screen.height / 2;

        currentHealth = maxHealth;
    }

    void Update() {
        if (!isDead) {
            CheckEnemyInFront();
        }

        // Fix the ******* bug when CharacterJokeController collides with rigidbodies.
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);
    }

    public void TakeDamage(int damage) {
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
    }

    public void ChangeMoney(int amount) {
        money += amount;
        UpdateMoneyUI();
    }

    void UpdateHealthUI() {
        healthBar.fillAmount = currentHealth / maxHealth;
        healthText.text = $"{currentHealth}/{maxHealth}";
    }

    void UpdateMoneyUI() {
        // no más de 9999999
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
            TakeDamage(1);
        }
    }
}