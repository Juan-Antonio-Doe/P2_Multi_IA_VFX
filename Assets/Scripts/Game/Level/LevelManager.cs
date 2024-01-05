using Nrjwolf.Tools.AttachAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour {

    [field: Header("AutoAttach on Editor properties")]
    [field: SerializeField, GetComponent, ReadOnlyField] private LevelTimer levelTimer { get; set; }    // Manages the level timer.
    //[field: SerializeField, FindObjectOfType, ReadOnlyField] private EnemyManager enemyManager { get; set; }    // Manages the enemies

    [field: Header("--- UI settings ---")]
    [field: SerializeField] private Text waveText { get; set; }

    [field: Header("--- Level manager properties ---")]
    [field: SerializeField] private float enemyWaveDurationMins { get; set; } = 8f;
    [field: SerializeField] private float enemyWaveIntervalSenconds { get; set; } = 15f;

    private float enemyWaveDurationSecs { get; set; }
    private bool isWaveActive { get; set; }
    private float currentTimeElapsed { get; set; }

    private int currentWave { get; set; } = 0;
    public int CurrentWave { get => currentWave; }


    [field: Header("Debug")]
    [field: SerializeField, ReadOnlyField] public static bool isStarted { get; private set; }

    private float width { get; set; } = Screen.width / 2;
    private float height { get; set; } = Screen.height / 2;

    void Start() {
        enemyWaveDurationSecs = enemyWaveDurationMins * 60;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.F4)) {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        if (Input.GetKeyDown(KeyCode.Return) && !isStarted) {
            StartGame();
        }

        if (!isStarted)
            return;

        WaveUpdate();
    }

    void StartGame() {
        isStarted = true;
    }

    void WaveUpdate() {
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
        waveText.text = currentWave.ToString();

        //Debug.Log($"Wave <color=red>{currentWave}</color> started.");

        //enemyManager.StartEnemyRespawn();
    }

    void EndCurrentWave() {
        //Debug.Log($"Wave Ended.");

        isWaveActive = false;
        currentTimeElapsed = 0f;

        //enemyManager.StopEnemyRespawn();
    }

    public void EndLevel() {
        isStarted = false;
        //enemyManager.StopWaves();
    }


    private void OnGUI() {
        if (!isStarted) {
            GUI.color = Color.red;
            GUI.skin.label.fontSize = 60;
            // Show text on the center of the screen.
            GUI.Label(new Rect(width - 250, height - 25, 1000, 1000),
                $"Máximo de juegadores: <color=green>3</color>\n" +
                $" - Pulse <b><color=clear>Enter</color></b> para empezar la partida.\n" +
                $" - Pulse <b><color=clear>F4</color></b> para salir.");
        }
    }
}