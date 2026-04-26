using System.Collections.Generic;
using UnityEngine;

public class CardDataManager
{
    static CardDataManager instance;
    public static CardDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new CardDataManager();
            }
            return instance;
        }
    }

    // ===== 완전히 분리된 두 풀 =====
    private Dictionary<string, PlayerCardData> playerCardDict = new Dictionary<string, PlayerCardData>();
    private Dictionary<string, MonsterCardData> monsterCardDict = new Dictionary<string, MonsterCardData>();

    // 레어도별 빠른 접근
    private Dictionary<CardRarity, List<PlayerCardData>> playerCardsByRarity = new Dictionary<CardRarity, List<PlayerCardData>>();
    private Dictionary<CardRarity, List<MonsterCardData>> monsterCardsByRarity = new Dictionary<CardRarity, List<MonsterCardData>>();


    // 외부(ExcelDataLoader)에서 접근 가능하도록 public
    public void RegisterPlayer(PlayerCardData data) => playerCardDict[data.cardID] = data;
    public void RegisterMonster(MonsterCardData data) => monsterCardDict[data.cardID] = data;

    public int PlayerCardCount => playerCardDict.Count;
    public int MonsterCardCount => monsterCardDict.Count;

    public void ClearPlayerCards()
    {
        playerCardDict.Clear();
        foreach (CardRarity r in System.Enum.GetValues(typeof(CardRarity)))
            playerCardsByRarity[r] = new System.Collections.Generic.List<PlayerCardData>();
    }

    public void ClearMonsterCards()
    {
        monsterCardDict.Clear();
        foreach (CardRarity r in System.Enum.GetValues(typeof(CardRarity)))
            monsterCardsByRarity[r] = new System.Collections.Generic.List<MonsterCardData>();
    }

    void GroupByRarity()
    {
        foreach (CardRarity rarity in System.Enum.GetValues(typeof(CardRarity)))
        {
            playerCardsByRarity[rarity] = new List<PlayerCardData>();
            monsterCardsByRarity[rarity] = new List<MonsterCardData>();
        }

        foreach (var card in playerCardDict.Values)
            playerCardsByRarity[card.rarity].Add(card);

        foreach (var card in monsterCardDict.Values)
            monsterCardsByRarity[card.rarity].Add(card);
    }

    // ========== 플레이어 카드 조회 ==========

    public PlayerCardData GetPlayerCard(string cardID)
    {
        if (playerCardDict.TryGetValue(cardID, out var data)) return data;
        LogHelper.LogError($"PlayerCardData not found: {cardID}");
        return null;
    }

    public List<PlayerCardData> GetPlayerCardsByRarity(CardRarity rarity)
        => playerCardsByRarity.TryGetValue(rarity, out var list) ? list : new List<PlayerCardData>();

    /// <summary>
    /// 가중치 기반으로 플레이어 카드 1장 뽑기
    /// </summary>
    public PlayerCardData RollPlayerCard(CardRarity rarity)
        => RollPlayerByWeight(GetPlayerCardsByRarity(rarity));

    PlayerCardData RollPlayerByWeight(List<PlayerCardData> pool)
    {
        if (pool == null || pool.Count == 0) return null;

        int total = 0;
        foreach (var card in pool) total += card.weight;

        int roll = Random.Range(0, total);
        int acc = 0;

        foreach (var card in pool)
        {
            acc += card.weight;
            if (roll < acc) return card;
        }

        return pool[pool.Count - 1];
    }

    // ========== 몬스터 카드 조회 ==========

    public MonsterCardData GetMonsterCard(string cardID)
    {
        if (monsterCardDict.TryGetValue(cardID, out var data)) return data;
        LogHelper.LogError($"MonsterCardData not found: {cardID}");
        return null;
    }

    public List<MonsterCardData> GetMonsterCardsByRarity(CardRarity rarity)
        => monsterCardsByRarity.TryGetValue(rarity, out var list) ? list : new List<MonsterCardData>();

    /// <summary>
    /// 가중치 기반으로 몬스터 카드 1장 뽑기
    /// </summary>
    public MonsterCardData RollMonsterCard(CardRarity rarity)
        => RollMonsterByWeight(GetMonsterCardsByRarity(rarity));

    MonsterCardData RollMonsterByWeight(List<MonsterCardData> pool)
    {
        if (pool == null || pool.Count == 0) return null;

        int total = 0;
        foreach (var card in pool) total += card.weight;

        int roll = Random.Range(0, total);
        int acc = 0;

        foreach (var card in pool)
        {
            acc += card.weight;
            if (roll < acc) return card;
        }

        return pool[pool.Count - 1];
    }
}
