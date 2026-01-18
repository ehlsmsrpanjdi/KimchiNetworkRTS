using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildingButtonsUI : UIBase
{
    [SerializeField] GridLayoutGroup gridLayoutGroup;
    [SerializeField] GameObject buildingButtonUIPrefab;
    [SerializeField] List<GameObject> buildingButtonUIs = new List<GameObject>();


    private void Reset()
    {
        gridLayoutGroup = GetComponent<GridLayoutGroup>();
        buildingButtonUIPrefab = Resources.Load<GameObject>("BuildingButton");
    }

    public void AddButton(int _ButtonData)
    {
        GameObject ButtonObj = Instantiate(buildingButtonUIPrefab, gridLayoutGroup.transform);
        BuildingButtonUI Button = ButtonObj.GetComponent<BuildingButtonUI>();

        buildingButtonUIs.Add(ButtonObj);

    }

    public void RemoveAllButton()
    {
        foreach (GameObject obj in buildingButtonUIs)
        {
            Destroy(obj);
        }
        buildingButtonUIs.Clear();
    }

}
