using UnityEngine;

[System.Serializable]
public class AugmentRarityConfig
{
    public float silverWeight = 50f;
    public float goldWeight = 30f;
    public float platinumWeight = 20f;

    public AugmentRarity RollRarity()
    {
        float total = silverWeight + goldWeight + platinumWeight;
        float roll = Random.Range(0f, total);

        if (roll < silverWeight)
            return AugmentRarity.Silver;
        else if (roll < silverWeight + goldWeight)
            return AugmentRarity.Gold;
        else
            return AugmentRarity.Platinum;
    }
}