using UnityEngine;
using UnityEngine.UI;

public class BuildingSystemButtonUI : UIBase
{
    [SerializeField] Button buildingSystemButton;

    BuildingSystemUI panel;

    private void Reset()
    {
        buildingSystemButton = GetComponent<Button>();
    }

    protected override void Start()
    {
        base.Start();
        panel = UIManager.Instance.GetUI<BuildingSystemUI>();

        buildingSystemButton.onClick.AddListener(panel.Toggle);
    }
}