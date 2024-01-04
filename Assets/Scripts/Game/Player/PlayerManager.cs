using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

	//[field: Header("Autottach on Editor properties")]

	[field: Header("Player Settings")]
	[field: SerializeField] private int maxHealth { get; set; } = 100;
	[field: SerializeField] private int currentHealth { get; set; } = 100;
	[field: SerializeField] private int money { get; set; } = 0;

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
        }
    }

    void Die() {
        isDead = true;
    }

    public void AddMoney(int amount) {
        money += amount;
    }
}