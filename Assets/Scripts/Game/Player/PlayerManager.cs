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

    [field: Header("Debug")]
    [field: SerializeField, ReadOnlyField] private bool isDead { get; set; }

    void Start() {
        currentHealth = maxHealth;
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
}