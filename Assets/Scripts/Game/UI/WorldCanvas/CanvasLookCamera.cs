using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasLookCamera : MonoBehaviour {

    [field: SerializeField] private Camera playerCamera { get; set; }

    void Start() {
        playerCamera = Camera.main;
    }

    void Update() {
        Quaternion lookRotation = playerCamera.transform.rotation;
        transform.rotation = lookRotation;
    }
}
