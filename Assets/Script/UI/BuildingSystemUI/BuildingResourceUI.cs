using TMPro;
using UnityEngine;

public class BuildingResourceUI : UIBase
{
    [SerializeField] TextMeshProUGUI resourceAmountText;

    private void Reset()
    {
        resourceAmountText = this.TryFindChild("ResourceAmountText").GetComponent<TextMeshProUGUI>();
    }

    protected override void Start()
    {
        base.Start();
    }

    void OnChangeResource(float _Amount)
    {
        resourceAmountText.text = $"Resources : {_Amount}";
    }
}
