using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerSpawn : MonoBehaviourPunCallbacks {

    [field: Header("Player spawn properties")]
    [field: SerializeField] private GameObject prefab { get; set; }
    private float yPos { get; set; } = 0f;

    [field: SerializeField] private GameObject[] spawnPoints { get; set; }


    //private int playerCount { get; set; } = 0;

    IEnumerator Start() {
        //PhotonNetwork.OfflineMode = true;
        Vector3 _spawnPos = GetSpawnPosition();

        Init();

        yield return new WaitForSeconds(0.5f); // Hay que a�adir este Delay para que funcione correctamente.
        // Crea un prefab para todos los usuarios conectados en la posicion indicada.
        GameObject playerGO = PhotonNetwork.Instantiate(prefab.name, _spawnPos, prefab.transform.rotation);
        //PlayerManager playerManager = playerGO.GetComponent<PlayerManager>();

        /*switch (PhotonNetwork.CurrentRoom.PlayerCount) {
            case 2:
                playerManager.render.material.color = Color.green;
                break;
            case 3:
                playerManager.render.material.color = Color.blue;
                break;
        }*/
    }

    void Init() {
        // Bloqueamos el cursor.
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public Vector3 GetSpawnPosition() {
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        Vector3 _spawnPos = spawnPoints[playerCount-1].transform.position;
        _spawnPos.y = yPos;
        /*playerCount++;

        if (playerCount > spawnPoints.Length - 1)
            playerCount = 0;*/

        return _spawnPos;
    }

    public override void OnLeftRoom() {
        base.OnLeftRoom();

        SceneManager.LoadScene(0);
    }
}
