using UnityEngine;

public class LayerHelper
{
    static LayerHelper instance;
    public static LayerHelper Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new LayerHelper();
            }
            return instance;
        }
    }

    public const string GridLayer = "Grid";
    public const string BuildingLayer = "Building";
    public const string EntityLayer = "Entity";
    public const string ObstacleLayer = "Obstacle";

    public int GetLayerToInt(string _str)
    {
        return 1 << LayerMask.NameToLayer(_str);
    }

    public string GetObjectLayer(GameObject _obj)
    {
        int layerIndex = _obj.layer;
        return LayerMask.LayerToName(_obj.layer);
    }
}
