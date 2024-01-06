using Nrjwolf.Tools.AttachAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapTriggerProxy : MonoBehaviour {
	
	[field: Header("Autoattach on Editor properties")]
	[field: SerializeField, GetComponentInParent, ReadOnlyField] private Trap trap { get; set; }

    [field: Header("Trap Trigger Proxy Settings")]
    [field: SerializeField] private float damage { get; set; } = 8f;

    private void OnTriggerEnter(Collider other) {
        if (!trap.IsPlaced)
            return;

        if (other.CompareTag("Enemy")) {
            other.GetComponent<Enemy>().TakeDamage(damage, trap.owner);
        }
    }
}