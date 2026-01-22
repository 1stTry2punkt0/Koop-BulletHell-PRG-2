using UnityEngine;
using TMPro;

public class WaveUI : MonoBehaviour
{
    // References to WaveController and UI Text components
    [Header("References")]
    [SerializeField] private WaveController waveController;
    [SerializeField] private TMP_Text waveCount;
    [SerializeField] private TMP_Text waveTimer;

    private void Update()
    {
        if (waveController == null) return;
        // Update wave count display
        waveCount.text = $"Wave: {waveController.CurrentWave}/{waveController.TotalWaves}";

        // handle boss waves 
        if (waveController.BossAlive)
        {
            UIManager.Instance.ActivateBossUI(waveController.BossName);
            waveTimer.gameObject.SetActive(waveController.BossUsesTimer);
        }
        else
        {
            UIManager.Instance.DisableBossUI();
            waveTimer.gameObject.SetActive(true);
        } 

        // Update wave timer display
        float remainingTime = waveController.RemainingWaveTime > 0
            ? waveController.RemainingWaveTime : waveController.TimeBetweenWaves; //choose which timer to show
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        if (waveController.RemainingWaveTime > 0)
            waveTimer.text = $"Time Left: {minutes:00}:{seconds:00}"; // show time left in current wave
        else if (waveController.TimeBetweenWaves > 0)
            waveTimer.text = $"Starting Wave In: {minutes:00}:{seconds:00}"; // show time until next wave
        else
            waveTimer.text = ""; //fallback

    }

}
