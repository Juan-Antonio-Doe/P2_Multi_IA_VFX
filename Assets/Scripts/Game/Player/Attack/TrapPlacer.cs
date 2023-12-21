using Nrjwolf.Tools.AttachAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapPlacer : MonoBehaviour
{
    [field: Header("Autoattach Settings")]
    [field: SerializeField, FindObjectOfType, ReadOnlyField] private Camera cam { get; set; }

    [field: Header("Trap Placer Settings")]
    // LayerMask para los obstaculos y otras trampas (6 = Obstacle y 7 = Trap)
    [field: SerializeField, ReadOnlyField] private LayerMask blockLayer { get; set; } = 1 << 6 | 1 << 7;
    [field: SerializeField, ReadOnlyField] private KeyCode enterTrapModeKey { get; set; } = KeyCode.T;
    [field: SerializeField, ReadOnlyField] private KeyCode placeTrapKey { get; set; } = KeyCode.Space;
    [field: SerializeField] private GameObject trapPrefab { get; set; }
    [field: SerializeField] private float rayLength { get; set; } = 2f;

    private Trap tempTrap { get; set; }

    void Update()
    {
        //Cambiamos al modo de colocar trampas
        if (Input.GetKeyDown(enterTrapModeKey))
        {
            TryPlaceTrap();
        }
        //Cuando la trampa temporal no es null, significa que estamos intentando colocar una trampa
        if (tempTrap != null)
        {
            //Movemos la trampa placeholder a la posicion del suelo en la que se colocaria
            tempTrap.transform.position = GetRoundedCenterGroundPos(tempTrap.transform.position.y);

            if (CanPlaceGroundTrap() == true)   //Si podemos colocar la trampa porque no hay ningun osbtaculo u otra trampa colocada...
            {
                if (Input.GetKeyDown(placeTrapKey)) //Usando la key de colocar trampa, ponemos la trampa en el suelo
                {
                    PlaceTrap();
                    tempTrap = null;
                }
            }
        }
    }

    void TryPlaceTrap()
    {
        switch(tempTrap == null) //Comprueba si la trampa placeholder existe o no para entrar/salir del modo de colocar trampa
        {
            //Si NO existe la trampla placeholder, la instancia para entrar al modo de colocar trampa y poder ver donde se colocara
            case true:
                tempTrap = Instantiate(trapPrefab, trapPrefab.transform.position, Quaternion.identity).GetComponent<Trap>();
                break;
            //Si SI existe la trampla placeholder, la destruye para salir del demodo colocar trampa
            case false:
                Destroy(tempTrap.gameObject);
                break;
        }
    }

    void PlaceTrap()
    {
        //Marcamos la trampa como colocada si no lo estaba aun
        if(tempTrap.isPlaced == false)
        {
            tempTrap.Place();
        }
    }

    //Comprueba con un CheckBox del mismo tamaño que la trampa si hay algun obstaculo u otra trampa colocada en la posicion donde queremos crear una trampa
    bool CanPlaceGroundTrap()
    {
        //Si hay algun obstaculo o trampa, pone el material de la trampa placeholder en rojo y devuelve false
        if (Physics.CheckBox(GetRoundedCenterGroundPos(tempTrap.transform.position.y), tempTrap.transform.localScale / 2.01f, tempTrap.transform.rotation, blockLayer))
        {
            tempTrap.SetHoloMatColor(Color.red);
            return false;
        }
        else    //Si la posicion esta libre, pone el material de la trampa placeholder en verde y devuelve true
        {
            tempTrap.SetHoloMatColor(Color.green);
            return true;
        }
    }

    //Calcula y devuelve la posicion del suelo en la que hay que mover la trampa para mostrar donde se colocaria
    Vector3 GetRoundedCenterGroundPos(float _yPos)
    {
        Vector3 _roundedPos = Vector3.zero;
        Ray _ray = cam.ViewportPointToRay(new Vector3(.5f, .5f, cam.nearClipPlane));
        Debug.DrawRay(_ray.origin, _ray.direction * rayLength, Color.red);
        Debug.DrawRay(_ray.origin + _ray.direction * rayLength, Vector3.down * 5f, Color.red);
        if (Physics.Raycast(_ray.origin, _ray.direction, out RaycastHit _hit, rayLength))
        {
            _roundedPos.x = Mathf.RoundToInt(_hit.point.x);
            _roundedPos.z = Mathf.RoundToInt(_hit.point.z);
        }
        else if (Physics.Raycast(_ray.origin + _ray.direction * rayLength, Vector3.down, out _hit, 5f))
        {
            _roundedPos.x = Mathf.RoundToInt(_hit.point.x);
            _roundedPos.z = Mathf.RoundToInt(_hit.point.z);
        }
        _roundedPos.y = _yPos;
        return _roundedPos;
    }

    private void OnDrawGizmos()
    {
        if(tempTrap != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(GetRoundedCenterGroundPos(tempTrap.transform.position.y), tempTrap.transform.localScale * 1.005f);
            Gizmos.DrawRay(transform.position, transform.forward * 1.9f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, transform.forward * 2.5f);
        }
    }
}
