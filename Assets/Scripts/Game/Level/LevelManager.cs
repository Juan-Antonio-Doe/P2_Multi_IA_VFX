using Nrjwolf.Tools.AttachAttributes;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour {

    [field: Header("AutoAttach on Editor properties")]
    [field: SerializeField, FindObjectOfType, ReadOnlyField] private EnemiesManager enemiesManager { get; set; }    // Manages the enemies.
    [field: SerializeField, GetComponent, ReadOnlyField] private LevelTimer levelTimer { get; set; }            // Manages the level timer.

    [field: Header("--- UI settings ---")]
    [field: SerializeField] private Text waveText { get; set; }
    [field: SerializeField] private GameObject victoryPanel { get; set; }
    [field: SerializeField] private GameObject gameOverPanel { get; set; }

    [field: Header("--- Level manager properties ---")]
    [field: SerializeField] private float enemyWaveDurationMins { get; set; } = 8f;
    [field: SerializeField] private float enemyWaveIntervalSenconds { get; set; } = 15f;
    [field: SerializeField] private int lastWave { get; set; } = 6;

    private float enemyWaveDurationSecs { get; set; }
    private bool isWaveActive { get; set; }
    private float currentTimeElapsed { get; set; }

    private int currentWave { get; set; } = 0;
    public int CurrentWave { get { return currentWave; } }


    [field: Header("Debug")]
    [field: SerializeField, ReadOnlyField] public static bool isStarted { get; private set; }
    [field: SerializeField, ReadOnlyField] public static bool isEnded { get; private set; }

    private float width { get; set; } = Screen.width / 2;
    private float height { get; set; } = Screen.height / 2;

    private string textBeforeStart { get; set; }

    void Start() {
        enemyWaveDurationSecs = enemyWaveDurationMins * 60;

        if (PhotonNetwork.IsMasterClient) {
            textBeforeStart = $"Máximo de jugadores: <color=clear>{PhotonNetwork.CurrentRoom.PlayerCount}</color>/<color=clear>3</color>\n" +
                $" - Pulse <b><color=clear>Enter</color></b> para empezar la partida.\n" +
                $" - Pulse <b><color=clear>F4</color></b> para salir.";
        }
        else {
            textBeforeStart = $"Máximo de jugadores: <color=clear>{PhotonNetwork.CurrentRoom.PlayerCount}</color>/<color=clear>3</color>\n" +
                $" - Esperando al anfitrión...";
        }
        
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.F4)) {
#if UNITY_EDITOR
            //UnityEditor.EditorApplication.isPlaying = false;
            PhotonNetwork.LeaveRoom();
#else
            //Application.Quit();
            PhotonNetwork.LeaveRoom();
#endif
        }

        if (Input.GetKeyDown(KeyCode.Return) && !isStarted && !isEnded && PhotonNetwork.IsMasterClient) {
            StartGame();
        }

        if (!isStarted)
            return;

        WaveUpdate();
    }

    void StartGame() {
        isStarted = true;
        PhotonNetwork.CurrentRoom.IsOpen = false;
    }

    void WaveUpdate() {
        if (isEnded)
            return;

        if (currentWave > lastWave) {
            EndLevel();
            if (!gameOverPanel.activeInHierarchy)
                victoryPanel.SetActive(true);
            return;
        }

        if (isWaveActive) {
            // Update the time elapsed
            currentTimeElapsed += Time.deltaTime;

            // If the time elapsed is greater than or equal to the wave duration, end the wave.
            if (currentTimeElapsed >= enemyWaveDurationSecs) {
                EndCurrentWave();
            }
        }
        else {
            // Update the time elapsed
            currentTimeElapsed += Time.deltaTime;

            // Check if the time elapsed is greater than or equal to the wave interval, start a new wave.
            if (currentTimeElapsed >= enemyWaveIntervalSenconds) {
                StartNewWave();
            }
        }

    }

    void StartNewWave() {

        isWaveActive = true;
        currentTimeElapsed = 0f;
        currentWave++;

        if (currentWave > lastWave) {
            return;
        }

        waveText.text = currentWave.ToString();

        //Debug.Log($"Wave <color=red>{currentWave}</color> started.");

        enemiesManager.StartEnemyRespawn();
    }

    void EndCurrentWave() {
        //Debug.Log($"Wave Ended.");

        isWaveActive = false;
        currentTimeElapsed = 0f;

        if (currentWave == lastWave)
            waveText.text = $"<color=green>Final wave completed!</color>";
        else if (currentWave == lastWave - 1)
            waveText.text = "Incoming final wave...";
        else
            waveText.text = $"Incoming wave {currentWave + 1}...";

        enemiesManager.StopEnemyRespawn();
    }

    public void EndLevel() {
        isStarted = false;
        enemiesManager.StopWaves();
        isEnded = true;
    }

    public void GameOver() {
        EndLevel();
        if (!victoryPanel.activeInHierarchy)
            gameOverPanel.SetActive(true);
    }

    private void OnGUI() {
        if (!isStarted && !isEnded) {
            GUI.color = Color.red;
            GUI.skin.label.fontSize = 60;
            // Show text on the center of the screen.
            GUI.Label(new Rect(width - 250, height - 25, 1000, 1000), textBeforeStart);
        }
    }
}