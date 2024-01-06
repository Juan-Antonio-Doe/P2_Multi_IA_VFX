using Nrjwolf.Tools.AttachAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Enemy : MonoBehaviour {

    [field: Header("Enemy settings")]
    [field: SerializeField, FindObjectOfType, ReadOnlyField] public EnemiesManager enemies { get; set; }
    [field: SerializeField, GetComponent, ReadOnlyField] protected Rigidbody rb { get; set; }
    public Rigidbody Rb { get { return rb; } }
    [field: SerializeField] protected float maxHealth { get; set; } = 20f;
    [field: SerializeField, ReadOnlyField] protected float health { get; set; } = 20f;
    [field: SerializeField] protected float damageReceivedByPlayer { get; set; } = 5f;
    [field: SerializeField] protected int moneyReward { get; set; } = 100;

    [field: Header("UI")]
    [field: SerializeField] protected Image healthBar { get; set; }
    [field: SerializeField, ReadOnlyField] protected Text healthText { get; set; }
    [field: SerializeField] protected GameObject enemyCanvasGO { get; set; }
    [field: SerializeField] protected float canvasDisplayTime { get; set; } = 2f;
    protected bool isPlayerLookingAtEnemy { get; set; }
    protected bool isHideCanvasCoActive { get; set; }

    [field: Header("Attack settings")]
    [field: SerializeField] protected float attackRange { get; set; } = 10f;
    public float AttackRange { get => attackRange; }
    [field: SerializeField] protected float attackCooldown { get; set; } = 1f;
    public float AttackCooldown { get { return attackCooldown; } set { attackCooldown = value; } }

    [field: Header("Move settings")]
    [field: SerializeField] private float moveSpeed { get; set; } = 4f;
    public float MoveSpeed { get => moveSpeed; }

    [field: Header("Debug")]
    [field: SerializeField, ReadOnlyField] protected Transform attackTarget { get; set; }
    public Transform AttackTarget { get { return attackTarget; } set { attackTarget = value; } }

    protected bool isDead { get; set; }
    public bool IsDead { get { return isDead; } set { isDead = value; } }

    public virtual void TakeDamage(float damage, PlayerManager killer=null) {
        if (health > 0) {
            health -= damage;
            if (health <= 0) {
                health = 0;
                Die(killer);
            }
            UpdateUI();
        }
    }

    protected virtual void Die(PlayerManager killer=null) {
        isDead = true;

        if (killer != null) {
            killer.ChangeMoney(moneyReward);
        }

        //gameObject.SetActive(false);
    }

    protected void UpdateUI() {
        healthBar.fillAmount = health / maxHealth;
        healthText.text = $"{health} / {maxHealth}";
    }
}