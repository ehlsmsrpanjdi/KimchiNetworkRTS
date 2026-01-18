using TMPro;
using UnityEngine;

public class GameStatePanelUI : UIBase
{
    [SerializeField] TextMeshProUGUI currentWave;
    [SerializeField] TextMeshProUGUI remainTime;
    [SerializeField] TextMeshProUGUI totalTime;
    [SerializeField] TextMeshProUGUI remainPlayer;


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
    }
}
