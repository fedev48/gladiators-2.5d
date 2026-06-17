using UnityEngine;
using TMPro;

public class FpsDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text fpsText;
    [SerializeField] private float updateInterval = 0.2f;
    [SerializeField] private bool showWorst = true;

    private float timer;
    private float worstDeltaInPeriod;

    void Update()
    {
        if (Time.unscaledDeltaTime > worstDeltaInPeriod)
            worstDeltaInPeriod = Time.unscaledDeltaTime;

        timer += Time.unscaledDeltaTime;
        if (timer < updateInterval) return;

        int fps = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);
        float ms = Time.unscaledDeltaTime * 1000f;

        string text = $"{fps} FPS ({ms:F1}ms)";

        if (showWorst)
        {
            int worstFps = Mathf.RoundToInt(1f / worstDeltaInPeriod);
            string color = worstFps >= 60 ? "green" : "red";
            text += $"\n<color={color}>min {worstFps} FPS</color>";
        }

        fpsText.text = text;

        timer = 0f;
        worstDeltaInPeriod = 0f;
    }
}
