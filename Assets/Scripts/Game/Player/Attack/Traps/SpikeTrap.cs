using Nrjwolf.Tools.AttachAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeTrap : Trap {

    [field: Header("Autoattach on Editor properties")]
    [field: SerializeField, GetComponent, ReadOnlyField] private Animator anim { get; set; }

    [field: Header("Spike Trap Settings")]
    [field: SerializeField] private float activeTime { get; set; } = 5f;
    [field: SerializeField] private float cooldownTime { get; set; } = 5f;

    private bool isActivated { get; set; }

    private void OnCollisionEnter(Collision collision) {
        if (!isPlaced)
            return;

        if (collision.gameObject.CompareTag("Enemy") && !isActivated) {
            StartCoroutine(ActivateSpikesCo());
        }
    }

    private IEnumerator ActivateSpikesCo() {
        isActivated = true;

        // Activa la animación de los pinchos subiendo
        anim.SetBool("IsActivated", true);

        yield return new WaitForSeconds(activeTime);

        // Activa la animación de los pinchos bajando
        anim.SetBool("IsActivated", false);

        yield return new WaitForSeconds(cooldownTime);

        isActivated = false;
    }
}