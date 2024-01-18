using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveTrap : Trap {

    [field: Header("Explosion Trap Settings")]
    [field: SerializeField] private LayerMask enemyLayer { get; set; } = 1 << 11;
    [field: SerializeField] private float explosionRadius { get; set; } = 3f;
    [field: SerializeField] private GameObject explosionVFXPrefab { get; set; }

    private bool detonated { get; set; }

    private void OnCollisionEnter(Collision collision) {
        if (!isPlaced)
            return;

        if (collision.gameObject.CompareTag("Enemy") && !detonated) {
            detonated = true;
            StartCoroutine(DamageExplosionCo());
        }
    }

    IEnumerator DamageExplosionCo() {

        Collider[] enemies = Physics.OverlapSphere(transform.position, explosionRadius, enemyLayer);
        foreach (Collider enemy in enemies) {
            enemy.GetComponent<Enemy>().TakeDamage(80);
            yield return null;
        }

        Instantiate(explosionVFXPrefab, transform.position, explosionVFXPrefab.transform.rotation);
        Destroy(this.gameObject);
    }

    private void OnDrawGizmos() {
        if (!isPlaced && Application.isPlaying) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }

    private void OnDestroy() {
        if (gameObject == null)
            return;
    }
}