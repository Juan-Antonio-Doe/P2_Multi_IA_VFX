using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LevelTimer : MonoBehaviour {

    [field: Header("AutoAttach on Editor properties")]
    [field: SerializeField, ReadOnlyField] private Text timerText { get; set; }
    [field: SerializeField] private bool revalidateProperties { get; set; }

    [field: Header("Timer properties")]
    [field: SerializeField, ReadOnlyField] private float _timeElapsed { get; set; } // Tiempo transcurrido en el nivel
    public float TimeElapsed {
        get { return _timeElapsed; }
        set { _timeElapsed = value; }
    }
    private static string _timeFormatted { get; set; }
    public static string TimeFormatted {
        get { return _timeFormatted; }
        set { _timeFormatted = value; }
    }

    private string mins { get; set; }
    private string seconds { get; set; }

    void OnValidate() {
#if UNITY_EDITOR
        UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        bool isValidPrefabStage = prefabStage != null && prefabStage.stageHandle.IsValid();
        bool prefabConnected = PrefabUtility.GetPrefabInstanceStatus(this.gameObject) == PrefabInstanceStatus.Connected;
        if (!isValidPrefabStage/* && prefabConnected*/) {
            //Variables que solo se verificaran cuando estan en una escena
            if (timerText == null || revalidateProperties) {
                revalidateProperties = false;
                timerText = GameObject.Find("TimerText").GetComponent<Text>();
            }
        }
#endif
    }

    void Update() {

        if (!LevelManager.isStarted)
            return;

        _timeElapsed += Time.deltaTime;
        TimerTextUpdate();
    }


    void TimerTextUpdate() {
        timerText.text = _timeFormatted = FormatTime(_timeElapsed);
    }

    string FormatTime(float timeElapsed) {
        mins = Mathf.Floor(timeElapsed / 60).ToString("00");
        seconds = (timeElapsed % 60).ToString("00");

        return string.Format("{0}:{1}", mins, seconds);
    }

}