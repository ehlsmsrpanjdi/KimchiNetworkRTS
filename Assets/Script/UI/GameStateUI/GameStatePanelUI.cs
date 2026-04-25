using TMPro;
using UnityEngine;

public class GameStatePanelUI : UIBase
{
    [SerializeField] TextMeshProUGUI currentWave;
    [SerializeField] TextMeshProUGUI remainTime;
    [SerializeField] TextMeshProUGUI totalTime;
    [SerializeField] TextMeshProUGUI remainPlayer;

    float elapsedTime;

    private void Reset()
    {
        currentWave = this.TryFindChild("CurrentWave").GetComponent<TextMeshProUGUI>();
        remainTime = this.TryFindChild("RemainTime").GetComponent<TextMeshProUGUI>();
        totalTime = this.TryFindChild("TotalTime").GetComponent<TextMeshProUGUI>();
        remainPlayer = this.TryFindChild("RemainPlayer").GetComponent<TextMeshProUGUI>();
    }

    protected override void Start()
    {
        base.Start();
        elapsedTime = 0f;
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.isGameStart.Value) return;

        elapsedTime += Time.deltaTime;

        if (WaveManager.Instance != null)
        {
            int wave = WaveManager.Instance.currentWaveNum.Value;
            currentWave.text = $"Wave {wave}";

            float countdown = WaveManager.Instance.nextWaveCountdown.Value;
            if (WaveManager.Instance.isWaveActive.Value)
                remainTime.text = "";
            else
                remainTime.text = $"Next: {Mathf.CeilToInt(countdown)}s";
        }

        int mins = Mathf.FloorToInt(elapsedTime / 60f);
        int secs = Mathf.FloorToInt(elapsedTime % 60f);
        totalTime.text = $"{mins:00}:{secs:00}";

        int alive = PlayerManager.Instance.GetAlivePlayers().Count;
        remainPlayer.text = $"Players: {alive}";
    }
}
