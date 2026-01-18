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


}
