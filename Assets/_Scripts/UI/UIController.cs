using System;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI timerText;


    void Start()
    {
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
