using Nrjwolf.Tools.AttachAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyRunner : Enemy {

    [field: Header("Autoattach properties")]
    [field: SerializeField, GetComponent, ReadOnlyField] protected NavMeshAgent agent { get; set; }
    [field: SerializeField] private bool revalidateProperties { get; set; } = false;

    private EnemyState currentState { get; set; }

    private Coroutine hideCanvasAfterTimeCo;

    private bool started { get; set; }

    void OnValidate() {
#if UNITY_EDITOR
        UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        bool isValidPrefabStage = prefabStage != null && prefabStage.stageHandle.IsValid();
        bool prefabConnected = PrefabUtility.GetPrefabInstanceStatus(this.gameObject) == PrefabInstanceStatus.Connected;
        if (!isValidPrefabStage && prefabConnected) {
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
        if (started) {
            ResetEnemy2();
        }

        // Suscribirse al evento cuando el objeto se activa
        PlayerManager.OnPlayerLookAtEnemy += ShowHideCanvas;
    }

    void OnDisable() {
        enemies.AddEnemyToPool(this);
        ResetEnemy1();

        // Anular la suscripción al evento cuando el objeto se desactiva
        PlayerManager.OnPlayerLookAtEnemy -= ShowHideCanvas;
    }

    void Start() {
        enemyCanvasGO.SetActive(false);

        currentState = new EnemyMovingToPlayerBaseState(this, agent);

        UpdateUI();

        started = true;
    }

    void Update() {
        if (!LevelManager.isStarted)
            return;

        currentState = currentState.Process();

        // Si el jugador deja de mirar al enemigo y el canvas está activo
        if (!isPlayerLookingAtEnemy && enemyCanvasGO.activeInHierarchy && !isHideCanvasCoActive) {
            // Comenzar la cuenta atrás para ocultar el canvas
            hideCanvasAfterTimeCo = StartCoroutine(HideCanvasAfterTimeCo(canvasDisplayTime));
        }
    }

    protected override void Die(PlayerManager killer) {
        base.Die(killer);
        //isDead = true;
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

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Projectile")) {
            TakeDamage(damageReceivedByPlayer, collision.gameObject.GetComponent<Projectile>().owner);
        }
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

    public void ResetEnemy1() {
        transform.position = enemies.transform.position;
        isDead = false;
        health = maxHealth;
        UpdateUI();

        if (hideCanvasAfterTimeCo != null) {
            StopCoroutine(hideCanvasAfterTimeCo);
            isHideCanvasCoActive = false;
        }
    }

    public void ResetEnemy2() {
        agent.enabled = true;
        currentState?.ChangeState(new EnemyMovingToPlayerBaseState(this, agent));
    }

}