using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody rb;

    private void Awake()
    {
        TryGetComponent(out rb);
    }

    void OnEnable()
    {
        //Para que funcione, hay que asegurarse de que el proyectil esta rotado para que mire hacia la direccion en la que queremos dispararlo
        rb.velocity = transform.forward * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}
