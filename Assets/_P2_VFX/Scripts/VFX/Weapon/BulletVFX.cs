using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletVFX : MonoBehaviour {

    [field : SerializeField] public Rigidbody rb { get; private set; }
	
    void Start() {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        rb.AddForce(transform.forward * 1000f);
        Destroy(gameObject, 5f);
    }

    private void OnCollisionEnter(Collision collision) {
        Destroy(gameObject);
    }
}