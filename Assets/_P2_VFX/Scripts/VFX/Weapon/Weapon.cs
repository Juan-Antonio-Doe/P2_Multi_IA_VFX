using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {

    [SerializeField] private float cameraSpeed = 10f;

    [SerializeField] private ParticleSystem shootPS;
    [SerializeField] private ParticleSystem impactPS;

    [field: SerializeField] private LayerMask impactLayers { get; set; }
    [field: SerializeField] private BulletVFX bullet { get; set; }
    [field: SerializeField] private Transform shootPoint { get; set; }
    [field: SerializeField] private float shootRate { get; set; }
    private float shootTimer { get; set; }

    void Update() {
        Vector3 _input = new Vector3(-Input.GetAxisRaw("Horizontal"), 0, 0);
        transform.Translate(_input * Time.deltaTime * cameraSpeed);

        if (Input.GetMouseButton(0)) {
            if (shootTimer <= 0f) {
                shootTimer = shootRate;
                Shoot();
            } else {
                shootTimer -= Time.deltaTime;
            }
        }
    }

    void Shoot() {
        shootPS.Play();

        if (bullet != null)
            Instantiate(bullet, shootPoint.position, shootPoint.rotation);

        if (Physics.Raycast(shootPS.transform.position, shootPS.transform.forward, out RaycastHit _hit, 100f, impactLayers)) {
            impactPS.transform.position = _hit.point + _hit.normal * 0.01f;
            impactPS.transform.rotation = Quaternion.LookRotation(_hit.normal);
            impactPS.Play();
        }
    }
}
