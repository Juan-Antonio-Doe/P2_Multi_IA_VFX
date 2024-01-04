using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [field: Header("Player Settings")]
    [field: SerializeField] private float speed = 3f;
    [field: SerializeField] private float rotSpeed = 400f;
    public Transform camPivot;  //Pivote de la camara, para la rotacion en el eje X
    public Transform shootOrigin;

    public GameObject projectilePrefab;
    public float shootRate = 3;
    public Camera cam; //Camara usada con el CineMachine

    private float xRot = 0f;
    private float shootTimer = 0f;

    private CharacterController controller;

    void Start()
    {
        TryGetComponent(out controller);
    }

    void Update()
    {
        MovementAndRotation();
        Shoot();
    }

    void MovementAndRotation()
    {
        //Calcula el Vector de movimiento a traves del input
        Vector3 _input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        //Transforma a espacio local para que cambie rotacion del personaje y mueve el CharacterController
        _input = transform.TransformDirection(_input);
        controller.Move(_input.normalized * speed * Time.deltaTime);

        //Rotacion en el eje Y con el input del raton hacia derecha - izquierda
        transform.Rotate(0f, Input.GetAxisRaw("Mouse X") * rotSpeed * Time.deltaTime, 0f);

        //Rotacion en el eje X, rota el pivote de la camara hacia arriba y hacia abajo con el input del raton
        xRot -= Input.GetAxisRaw("Mouse Y") * rotSpeed * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, -90f, 90f);
        camPivot.localEulerAngles = new Vector3(xRot, camPivot.localEulerAngles.y, camPivot.localEulerAngles.z);
    }

    void Shoot()
    {
        if (Input.GetMouseButton(0) && Time.time >= shootTimer)
        {
            //Reiniciamos el timer de disparo
            shootTimer = Time.time + (1f / shootRate);

            //Calculamos y lanzamos un rayo desde la camara hacia el centro de la pantalla
            Ray _ray = cam.ViewportPointToRay(new Vector3(.5f, .5f, cam.nearClipPlane));

            if(Physics.Raycast(_ray, out RaycastHit _hit, 1000f))
            {
                //Calculamos la direccion en la que tiene que ir el proyectil a partir de donde ha impactado el Raycast
                Vector3 _projectileDir = _hit.point - shootOrigin.position;
                //Creamos el proyectil en el punto de origen y lo rotamos para que mire hacia la direccion en la que lo disparamos
                CreateProjectile(shootOrigin.position, Quaternion.LookRotation(_projectileDir));
            }
            else //Si el rayo no golpea contra nada, simplemente disparamos el proyectil en la misma direccion del rayo
            {
                //Creamos el proyectil en el punto de origen y lo rotamos para que mire hacia la direccion en la que lo disparamos
                CreateProjectile(shootOrigin.position, Quaternion.LookRotation(_ray.direction));
            }
        }
    }

    void CreateProjectile(Vector3 _position, Quaternion _rotation)
    {
        //El propio proyectil lleva un script que hace que se a�ada velocity en su transform.forward, por eso rotamos el proyectil
        //para que mire hacia la direccion en la que lo disparamos
        Instantiate(projectilePrefab, _position, _rotation);
    }

    private void OnValidate()
    {
        if(shootTimer <= 0f)
        {
            shootTimer = .1f;
        }
    }
}