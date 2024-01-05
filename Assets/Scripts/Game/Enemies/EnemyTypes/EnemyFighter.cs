using Nrjwolf.Tools.AttachAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyFighter : Enemy {

    [field: Header("Autoattach properties")]
    [field: SerializeField, GetComponent, ReadOnlyField] protected NavMeshAgent agent { get; set; }
    [field: SerializeField] private bool revalidateProperties { get; set; } = false;

    [field: Header("Attack settings")]
    [field: SerializeField] private LayerMask targetMasks { get; set; }
    private float defaultAttackCooldown { get; set; }

    private EnemyState currentState { get; set; }

    private float centerWidth { get; set; }
    private float centerHeight { get; set; }

    private Coroutine hideCanvasAfterTimeCo;

    void OnValidate() {
#if UNITY_EDITOR
        UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        bool isValidPrefabStage = prefabStage != null && prefabStage.stageHandle.IsValid();
        bool prefabConnected = PrefabUtility.GetPrefabInstanceStatus(this.gameObject) == PrefabInstanceStatus.Connected;
        if (!isValidPrefabStage/* && prefabConnected*/) {
            // Variables that will only be checked when they are in a scene
            if (!Application.isPlaying) {
                if (revalidateProperties)
                    Validate();
            }
        }

#endif
    }

    void Validate() {
        if (healthText == null || revalidateProperties) {
            healthText = healthBar.transform.GetChild(0).GetComponent<Text>();
        }

        if (enemyCanvasGO == null || revalidateProperties) {
            enemyCanvasGO = healthBar.gameObject.GetComponentInParent<Canvas>().gameObject;
        }
        revalidateProperties = false;
    }

    void OnEnable() {
        // Suscribirse al evento cuando el objeto se activa
        PlayerManager.OnPlayerLookAtEnemy += ShowHideCanvas;
    }

    void OnDisable() {
        // Anular la suscripción al evento cuando el objeto se desactiva
        PlayerManager.OnPlayerLookAtEnemy -= ShowHideCanvas;
    }

    void Start() {
        enemyCanvasGO.SetActive(false);
        centerWidth = Screen.width / 2;
        centerHeight = Screen.height / 2;
        defaultAttackCooldown = attackCooldown;

        currentState = new EnemyMovingToPlayerBaseState(this, agent);
    }

    void Update() {
        if (!LevelManager.isStarted)
            return;

        currentState = currentState.Process();

        if (!isDead) {
            if (attackTarget != null) {
                if (!attackTarget.gameObject.activeInHierarchy) {
                    attackTarget = null;
                }
            }

            if (attackTarget == null) {
                CheckNearby();
            }
        }

        // Si el jugador deja de mirar al enemigo y el canvas está activo
        if (!isPlayerLookingAtEnemy && enemyCanvasGO.activeInHierarchy && !isHideCanvasCoActive) {
            // Comenzar la cuenta atrás para ocultar el canvas
            hideCanvasAfterTimeCo = StartCoroutine(HideCanvasAfterTimeCo(canvasDisplayTime));
        }
    }

    public void CheckNearby() {
        // Detect the firt ally NPC or Player that is on attack range using OverlapSphere.
        Collider[] colliders = Physics.OverlapSphere(transform.position, attackRange, targetMasks);
        attackTarget = null; // Reset the attack target

        foreach (Collider collider in colliders) {
            if (collider.gameObject != gameObject && collider.gameObject.activeInHierarchy) {
                attackTarget = collider.gameObject.transform;
                return; // Stop the loop once the first valid target is found
            }
        }
    }

    protected override void Die() {
        //base.Die();
        isDead = true;
    }

    void ShowHideCanvas(Transform enemyTransform) {
        // Firts attempt to show the canvas

        // Si el jugador está mirando a este enemigo y la vida del enemigo no está completa
        if (enemyTransform == transform && health < maxHealth) {
            // Activar el canvas de salud
            enemyCanvasGO.SetActive(true);
            // Indicar que el jugador está mirando al enemigo
            isPlayerLookingAtEnemy = true;
        }
        /*else {  // No llega a llamarse.
            // Indicar que el jugador no está mirando al enemigo
            isPlayerLookingAtEnemy = false;
        }*/

        isPlayerLookingAtEnemy = false;
    }

    IEnumerator HideCanvasAfterTimeCo(float time) {
        isHideCanvasCoActive = true;

        // Esperar el tiempo especificado
        yield return new WaitForSeconds(time);

        // Desactivar el canvas de salud
        enemyCanvasGO.SetActive(false);
        isHideCanvasCoActive = false;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    private void OnBecameInvisible() {
        if (!Application.isPlaying)
            return;

        if (enemyCanvasGO.activeInHierarchy) {
            enemyCanvasGO.SetActive(false);

            if (hideCanvasAfterTimeCo != null) {
                StopCoroutine(hideCanvasAfterTimeCo);
                isHideCanvasCoActive = false;
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            // Start attack cooldown
            attackCooldown = defaultAttackCooldown;
        }
    }
}