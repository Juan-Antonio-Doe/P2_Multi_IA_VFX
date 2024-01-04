using Nrjwolf.Tools.AttachAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    [field: Header("Autoattach on Editor properties")]
    [field: SerializeField, GetComponent, ReadOnlyField] private Rigidbody rb { get; set; }

    [field: Header("Projectile Settings")]
    [field: SerializeField] private float speed { get; set; } = 5f;
    [field: SerializeField] private float lifeTime { get; set; } = 5f;

    void OnEnable() {
        //Para que funcione, hay que asegurarse de que el proyectil esta rotado para que mire hacia la direccion en la que queremos dispararlo
        rb.velocity = transform.forward * speed;
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision collision) {
        Destroy(gameObject);
    }

    void OnDestroy() {
        if (gameObject == null)
            return;
    }
}
