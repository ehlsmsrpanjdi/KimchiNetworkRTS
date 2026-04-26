using UnityEngine;

[System.Serializable]
public class CardRarityConfig
{
    public float commonWeight = 100f;
    public float uncommonWeight = 60f;
    public float rareWeight = 25f;
    public float epicWeight = 10f;

    public CardRarity RollRarity()
    {
        float total = commonWeight + uncommonWeight + rareWeight + epicWeight;
        float roll = Random.Range(0f, total);

        if (roll < commonWeight) return CardRarity.Common;
        if (roll < commonWeight + uncommonWeight) return CardRarity.Uncommon;
        if (roll < commonWeight + uncommonWeight + rareWeight) return CardRarity.Rare;
        return CardRarity.Epic;
    }
}
