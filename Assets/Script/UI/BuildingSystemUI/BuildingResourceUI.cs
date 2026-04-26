using System.Collections;
using TMPro;
using UnityEngine;

public class BuildingResourceUI : UIBase
{
    [SerializeField] TextMeshProUGUI resourceAmountText;

    private void Reset()
    {
        resourceAmountText = this.TryFindChild("ResourceAmountText").GetComponent<TextMeshProUGUI>();
    }

    protected override void Awake()
    {
        base.Awake();
        if (resourceAmountText == null)
            resourceAmountText = GetComponentInChildren<TextMeshProUGUI>();
    }

    protected override void Start()
    {
        base.Start();
        StartCoroutine(WaitAndSubscribe());
    }

    IEnumerator WaitAndSubscribe()
    {
        Player localPlayer = null;
        while (localPlayer == null)
        {
            localPlayer = PlayerManager.Instance.GetLocalPlayer();
            yield return null;
        }

        localPlayer.resource.OnResourceChanged += UpdateText;
        UpdateText(localPlayer.resource.GetResource());
    }

    void UpdateText(int amount)
    {
        resourceAmountText.text = $"Iron: {amount}";
    }

    private void OnDestroy()
    {
        var localPlayer = PlayerManager.Instance.GetLocalPlayer();
        if (localPlayer != null)
            localPlayer.resource.OnResourceChanged -= UpdateText;
    }
}
