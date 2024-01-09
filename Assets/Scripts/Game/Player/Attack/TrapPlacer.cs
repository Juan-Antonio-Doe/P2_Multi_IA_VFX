using ExitGames.Client.Photon;
using Nrjwolf.Tools.AttachAttributes;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TrapPlacer : MonoBehaviour, IOnEventCallback {

    [field: Header("Autoattach Settings")]
    [field: SerializeField, FindObjectOfType, ReadOnlyField] public Camera cam { get; set; }
    [field: SerializeField, GetComponent, ReadOnlyField] private PlayerManager playerManager { get; set; }
    [field: SerializeField, ReadOnlyField] public Text moneyCostText { get; set; }
    [field: SerializeField] private bool revalidateProperties { get; set; } = false;

    [field: Header("Trap Placer Settings")]
    // LayerMask para los obstaculos y otras trampas (6 = Obstacle y 7 = Trap)
    [field: SerializeField, ReadOnlyField] private LayerMask blockLayer { get; set; } = 1 << 6 | 1 << 7;
    [field: SerializeField, ReadOnlyField] private KeyCode enterTrapModeKey { get; set; } = KeyCode.T;
    [field: SerializeField, ReadOnlyField] private KeyCode placeTrapKey { get; set; } = KeyCode.Space;
    [field: SerializeField] private Trap[] trapPrefabs { get; set; }
    [field: SerializeField] private float rayLength { get; set; } = 2f;

    private Trap tempTrap { get; set; }
    private int currentTrapIndex { get; set; }

    // ---------------- Multiplayer ----------------

    public const int TRAP_SYNC_INSTANTIATION = 61, TRAP_SYNC_PLACE = 62;

    void OnValidate() {
#if UNITY_EDITOR
        UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        bool isValidPrefabStage = prefabStage != null && prefabStage.stageHandle.IsValid();
        bool prefabConnected = PrefabUtility.GetPrefabInstanceStatus(this.gameObject) == PrefabInstanceStatus.Connected;
        if (!isValidPrefabStage && prefabConnected) {
            // Variables that will only be checked when they are in a scene
            if (!Application.isPlaying) {
                if (revalidateProperties)
                    Validate();
            }
        }

#endif
    }

    void Validate() {
        if (moneyCostText == null || revalidateProperties) {
            moneyCostText = GameObject.Find("MoneyTrapCostText").GetComponent<Text>();
        }
        revalidateProperties = false;
    }

    void Start() {
        PhotonNetwork.AddCallbackTarget(this);
    }

    void Update() {
        if (!LevelManager.isStarted)
            return;

        if (!playerManager.photonView.IsMine)
            return;

        // Mostramos el coste de la trampa temporal en el texto junto con el nombre de la trampa acortando el texto "(Clone)"
        if (moneyCostText != null)
            moneyCostText.text = tempTrap != null ? $"{tempTrap.name.Replace("(Clone)", "")} | - {tempTrap.MoneyCost}" : "";

        //Cambiamos al modo de colocar trampas
        if (Input.GetKeyDown(enterTrapModeKey)) {
            TryPlaceTrap();
        }

        //Cuando la trampa temporal no es null, significa que estamos intentando colocar una trampa
        if (tempTrap != null) {
            // Switch trap with mouse wheel
            if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
            {
                SwichTrap(true);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
            {
                SwichTrap(false);
            }

            //Movemos la trampa placeholder a la posicion del suelo en la que se colocaria
            tempTrap.transform.position = GetRoundedCenterGroundPos(tempTrap.transform.position.y);

            if (CanPlaceGroundTrap() == true)   //Si podemos colocar la trampa porque no hay ningun osbtaculo u otra trampa colocada...
            {
                if (Input.GetKeyDown(placeTrapKey)) //Usando la key de colocar trampa, ponemos la trampa en el suelo
                {
                    // Here call event for place trap
                    PlaceTrap();
                    PhotonNetwork.RaiseEvent(TRAP_SYNC_PLACE, null, new RaiseEventOptions { Receivers = ReceiverGroup.Others }, SendOptions.SendReliable);
                    tempTrap = null;
                }
            }
        }
    }

    void TryPlaceTrap() {
        switch (tempTrap == null) //Comprueba si la trampa placeholder existe o no para entrar/salir del modo de colocar trampa
        {
            //Si NO existe la trampla placeholder, la instancia para entrar al modo de colocar trampa y poder ver donde se colocara
            case true:
                //InstantiateTrap(currentTrapIndex);
                PhotonNetwork.RaiseEvent(TRAP_SYNC_INSTANTIATION, currentTrapIndex, new RaiseEventOptions { Receivers = ReceiverGroup.All }, 
                    SendOptions.SendReliable);
                break;
            //Si SI existe la trampla placeholder, la destruye para salir del modo colocar trampa
            case false:
                Destroy(tempTrap.gameObject);
                break;
        }
    }

    void PlaceTrap() {
        //Marcamos la trampa como colocada si no lo estaba aun
        if (tempTrap.IsPlaced == false) {
            tempTrap.owner = playerManager;
            playerManager.ChangeMoney(-tempTrap.MoneyCost);
            tempTrap.Place();
        }
        // AJUSTAR ESTE MÉTODO PARA RECIBIR LA POSICIÓN CUANDO SE USA ONLINE
    }

    bool CanPlaceMoneyTrap() {
        if (LevelManager.DebugMode)
            return true;

        if (playerManager.Money >= tempTrap.MoneyCost) {
            tempTrap.SetHoloMatColor(Color.green);
            return true;
        }
        else {
            tempTrap.SetHoloMatColor(Color.grey);
            return false;
        }
    }

    //Comprueba con un CheckBox del mismo tamaño que la trampa si hay algun obstaculo u otra trampa colocada en la posicion donde queremos crear una trampa
    bool CanPlaceGroundTrap() {
        if (!CanPlaceMoneyTrap())
            return false;

        //Si hay algun obstaculo o trampa, pone el material de la trampa placeholder en rojo y devuelve false
        if (Physics.CheckBox(GetRoundedCenterGroundPos(tempTrap.transform.position.y), tempTrap.transform.localScale / 2.01f, tempTrap.transform.rotation, blockLayer)) {
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
    Vector3 GetRoundedCenterGroundPos(float _yPos) {
        Vector3 _roundedPos = Vector3.zero;
        Ray _ray = cam.ViewportPointToRay(new Vector3(.5f, .5f, cam.nearClipPlane));
        Debug.DrawRay(_ray.origin, _ray.direction * rayLength, Color.red);
        Debug.DrawRay(_ray.origin + _ray.direction * rayLength, Vector3.down * 5f, Color.red);
        if (Physics.Raycast(_ray.origin, _ray.direction, out RaycastHit _hit, rayLength)) {
            _roundedPos.x = Mathf.RoundToInt(_hit.point.x);
            _roundedPos.z = Mathf.RoundToInt(_hit.point.z);
        }
        else if (Physics.Raycast(_ray.origin + _ray.direction * rayLength, Vector3.down, out _hit, 5f)) {
            _roundedPos.x = Mathf.RoundToInt(_hit.point.x);
            _roundedPos.z = Mathf.RoundToInt(_hit.point.z);
        }
        _roundedPos.y = _yPos;
        return _roundedPos;
    }

    void SwichTrap(bool forwardDir) {
        if (forwardDir) {
            currentTrapIndex++;
            if (currentTrapIndex >= trapPrefabs.Length) {
                currentTrapIndex = 0;
            }
        }
        else {
            currentTrapIndex--;
            if (currentTrapIndex < 0) {
                currentTrapIndex = trapPrefabs.Length - 1;
            }
        }
        Destroy(tempTrap.gameObject);
        //InstantiateTrap(currentTrapIndex);
        PhotonNetwork.RaiseEvent(TRAP_SYNC_INSTANTIATION, currentTrapIndex, new RaiseEventOptions { Receivers = ReceiverGroup.All },
            SendOptions.SendReliable);
    }

    // ---------------- Multiplayer-Adapted Methods ----------------
    private void InstantiateTrap(int _currentTrapIndex) {
        tempTrap = Instantiate(trapPrefabs[_currentTrapIndex], trapPrefabs[_currentTrapIndex].transform.position, Quaternion.identity)/*.GetComponent<Trap>()*/;
    }

    private void OnDrawGizmos() {
        if (tempTrap != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(GetRoundedCenterGroundPos(tempTrap.transform.position.y), tempTrap.transform.localScale * 1.005f);
            Gizmos.DrawRay(transform.position, transform.forward * 1.9f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, transform.forward * 2.5f);
        }
    }

    public void OnEvent(EventData photonEvent) {
        if (photonEvent.Code == TRAP_SYNC_INSTANTIATION) {
            //if (!playerManager.photonView.IsMine)
                InstantiateTrap((int)photonEvent.CustomData);
        }

        if (photonEvent.Code == TRAP_SYNC_PLACE) {
            if (!playerManager.photonView.IsMine)
                PlaceTrap();
        }
    }
}
