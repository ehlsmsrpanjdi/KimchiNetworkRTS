using UnityEngine;
using UnityEngine.UI;

public class BuildingElementUI : UIBase
{
    [SerializeField] Button battleBuildingButton;
    [SerializeField] Button resourceBuildingButton;
    [SerializeField] Button wallBuildingButton;

    BuildingButtonsUI buildingButtonsUI;

    private void Reset()
    {
        battleBuildingButton = this.TryFindChild("BattleButton").GetComponent<Button>();
        resourceBuildingButton = this.TryFindChild("ResourceButton").GetComponent<Button>();
        wallBuildingButton = this.TryFindChild("WallButton").GetComponent<Button>();
    }

    protected override void Start()
    {
        base.Start();

        battleBuildingButton.onClick.AddListener(OnClickBattle);
        resourceBuildingButton.onClick.AddListener(OnClickResource);
        wallBuildingButton.onClick.AddListener(OnClickWall);

        buildingButtonsUI = UIManager.Instance.GetUI<BuildingButtonsUI>();
    }

    void OnClickBattle()
    {
        buildingButtonsUI.RemoveAllButton();
        buildingButtonsUI.ShowBuildingsByCategory(BuildingCategory.Attack);
    }

    void OnClickResource()
    {
        buildingButtonsUI.RemoveAllButton();
        buildingButtonsUI.ShowBuildingsByCategory(BuildingCategory.Resource);
    }

    void OnClickWall()
    {
        buildingButtonsUI.RemoveAllButton();
        buildingButtonsUI.ShowBuildingsByCategory(BuildingCategory.Wall);
    }
}