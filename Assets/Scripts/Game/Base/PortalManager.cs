using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PortalManager : MonoBehaviour {

    //[field: Header("Autoattach on Editor properties")]

    [field: Header("Portal Settings")]
    [field: SerializeField] private int maxHealth { get; set; } = 100;
    [field: SerializeField, ReadOnlyField] private int health { get; set; } = 100;
    [field: SerializeField] private int damageReceivedByEnemy { get; set; } = 10;

    [field: Header("UI Settings")]
    [field: SerializeField] private Image healthBar { get; set; }
    [field: SerializeField] private Text healthText { get; set; }

    [field: Header("Portal Die Event \n")]
    [field: SerializeField] private UnityEvent onPortalDie { get; set; }

    [field: Header("Debug")]
    [field: SerializeField] private bool debug { get; set; }

    void Start() {
        health = maxHealth;

        if (debug || LevelManager.DebugMode) {
            health = maxHealth = 10000;
        }

        UpdateHealthUI();
    }

    public void TakeDamage(int damage) {
        if (health > 0) {
            health -= damage;
            if (health <= 0) {
                health = 0;
                Die();
            }
            UpdateHealthUI();
        }
    }

    void Die() {
        // Game Over
        onPortalDie?.Invoke();
    }

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Enemy")) {
            TakeDamage(damageReceivedByEnemy);
            other.gameObject.SetActive(false);
        }
    }

    void UpdateHealthUI() {
        healthBar.fillAmount = (float) health / maxHealth;
        healthText.text = $"{health} / {maxHealth}";
    }
}