using System;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;

    void Start()
    {
        int targetHeight = 480;
        int targetWidth = Mathf.RoundToInt(targetHeight * (float)Screen.width / Screen.height);
        Screen.SetResolution(targetWidth, targetHeight, FullScreenMode.FullScreenWindow);

        Application.targetFrameRate = 300;
        QualitySettings.vSyncCount = 0;

        GameManager.Instance.gameAuthoritativeState.OnValueChanged += GameManager_OnGameStateChanged;
        GameManager.Instance.roundTimer.OnValueChanged += GameManager_OnRoundTimerChanged;
    }

    private void GameManager_OnRoundTimerChanged(int previousValue, int newValue)
    {
        timerText.text = newValue.ToString();
    }

    private void GameManager_OnGameStateChanged(GameState previousValue, GameState newValue)
    {
       if (newValue==GameState.InRound)
        {
            timerText.gameObject.SetActive(true);
        }
        else
        {
            timerText.gameObject.SetActive(false);
        }
    }
}
