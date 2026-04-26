using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 웨이브 종료 후 카드 선택 시스템
///
/// [흐름]
/// 웨이브 종료
///   → 서버: 레어도 뽑기
///   → 서버: 플레이어 카드 3장 옵션 생성 → 클라이언트에 UI 표시
///   → 플레이어: 1장 선택 → SelectPlayerCardServerRpc
///   → 서버: 선택된 플레이어 카드를 해당 플레이어 건물에 적용
///           + 동시에 몬스터 카드 1장을 같은 레어도에서 자동 뽑아 누적
///
/// [분리 원칙]
/// - PlayerCardData (Cards_Building): 플레이어만 획득, 건물에 적용
/// - MonsterCardData (Cards_Monster): 서버가 자동 뽑기, 몬스터에 누적
/// - 두 풀은 완전히 별개, 서로 영향 없음
/// </summary>
public class CardManager : NetworkBehaviour
{
    public static CardManager Instance;

    [Header("Rarity Config")]
    public CardRarityConfig rarityConfig = new CardRarityConfig();

    // ===== 플레이어 카드 상태 =====
    // 현재 웨이브 선택지 (서버만 보유)
    private List<PlayerCardData> currentOptions = new List<PlayerCardData>(3);

    // 플레이어별 획득한 카드 누적 (cardID → 획득 횟수)
    private Dictionary<ulong, Dictionary<string, int>> playerCardStacks = new Dictionary<ulong, Dictionary<string, int>>();

    // ===== 몬스터 카드 상태 =====
    // 누적된 몬스터 카드 (다음 웨이브 몬스터부터 전부 적용)
    private List<MonsterCardData> monsterCardAccumulated = new List<MonsterCardData>();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ====================================================
    // 웨이브 종료 시 WaveManager에서 호출
    // ====================================================

    [Rpc(SendTo.Server)]
    public void OnWaveEndServerRpc(int cardChoiceCount)
    {
        if (!IsServer) return;

        // 1. 레어도 뽑기 (플레이어/몬스터 공통으로 같은 레어도 적용)
        CardRarity rarity = rarityConfig.RollRarity();
        LogHelper.Log($"🎲 Rolled rarity: {rarity}");

        // 2. 플레이어 카드 선택지 준비 → UI 표시
        PreparePlayerCardOptions(rarity, cardChoiceCount);

        // 3. 몬스터 카드는 플레이어 선택 완료 시 같이 처리 (SelectPlayerCardServerRpc 참고)
        //    레어도를 함께 전달하기 위해 현재 레어도를 저장
        pendingMonsterCardRarity = rarity;
    }

    // 플레이어 선택 대기 중인 레어도 (서버 전용)
    private CardRarity pendingMonsterCardRarity;

    // ====================================================
    // 플레이어 카드 선택지 준비
    // ====================================================

    void PreparePlayerCardOptions(CardRarity rarity, int count)
    {
        List<PlayerCardData> pool = CardDataManager.Instance.GetPlayerCardsByRarity(rarity);
        if (pool.Count == 0)
        {
            LogHelper.LogError($"No player cards for rarity {rarity}!");
            return;
        }

        currentOptions.Clear();
        int optionCount = Mathf.Min(count, pool.Count);

        for (int i = 0; i < optionCount; i++)
        {
            currentOptions.Add(CardDataManager.Instance.RollPlayerCard(rarity));
        }

        // TODO: 실제 UI 연동 시 cardID 목록을 ClientRpc로 전달
        string optionNames = string.Join(", ", currentOptions.ConvertAll(c => c.displayName));
        LogHelper.Log($"📋 Player card options: {optionNames}");

        // 임시: 서버가 자동 선택
        SelectPlayerCardServerRpc(0, NetworkManager.Singleton.LocalClientId);
    }

    // ====================================================
    // 플레이어 카드 선택
    // ====================================================

    [Rpc(SendTo.Server)]
    public void SelectPlayerCardServerRpc(int optionIndex, ulong playerID)
    {
        if (!IsServer) return;
        if (optionIndex < 0 || optionIndex >= currentOptions.Count) return;

        PlayerCardData selected = currentOptions[optionIndex];
        LogHelper.Log($"✅ Player {playerID} selected: [{selected.rarity}] {selected.displayName}");

        // 플레이어 카드 → 해당 플레이어 건물에 적용
        ApplyPlayerCard(selected, playerID);

        // 몬스터 카드 → 같은 레어도에서 자동 뽑기 후 누적
        // 플레이어 선택과 동시에 이루어지지만 완전히 별개의 풀에서 처리
        DrawAndAccumulateMonsterCard(pendingMonsterCardRarity);

        currentOptions.Clear();
    }

    // ====================================================
    // 플레이어 카드 적용 로직
    // ====================================================

    void ApplyPlayerCard(PlayerCardData card, ulong playerID)
    {
        Player player = PlayerManager.Instance.GetPlayer(playerID);
        if (player == null) { LogHelper.LogError($"Player not found: {playerID}"); return; }

        // 스택 관리
        if (!playerCardStacks.ContainsKey(playerID))
            playerCardStacks[playerID] = new Dictionary<string, int>();

        int stackCount = playerCardStacks[playerID].GetValueOrDefault(card.cardID, 0);
        playerCardStacks[playerID][card.cardID] = stackCount + 1;

        // 타워 건물에 적용
        List<BuildingBase> towers = player.GetBuildingsByType(BuildingCategory.Tower);

        if (card.IsStatCard)
        {
            ApplyPlayerStatCard(card, towers, stackCount);
        }
        else if (card.IsBehaviorCard)
        {
            ApplyPlayerBehaviorCard(card, towers);
        }

        LogHelper.Log($"📈 Player card applied to {playerID}: {card.displayName} (stack {stackCount + 1})");
    }

    void ApplyPlayerStatCard(PlayerCardData card, List<BuildingBase> buildings, int stackCount)
    {
        string modID = $"{card.cardID}_stack{stackCount}";
        string modName = $"{card.displayName} (Stack {stackCount + 1})";

        StatModifier modifier = CreateStatModifier(card.cardID, card.op, card.value, modID, modName);
        if (modifier == null) return;

        foreach (var building in buildings)
            building.ApplyStatModifier(modifier);
    }

    void ApplyPlayerBehaviorCard(PlayerCardData card, List<BuildingBase> buildings)
    {
        foreach (var building in buildings)
        {
            IEventModifier eventModifier = CreateBehaviorModifier(card.cardID, card.displayName, card.statKey, card.effectID);
            if (eventModifier != null)
                building.ApplyEventModifier(eventModifier);
        }

        LogHelper.Log($"⚡ Behavior card applied: {card.statKey}");
    }

    // ====================================================
    // 몬스터 카드 자동 뽑기 & 누적
    // ====================================================

    void DrawAndAccumulateMonsterCard(CardRarity rarity)
    {
        MonsterCardData drawn = CardDataManager.Instance.RollMonsterCard(rarity);
        if (drawn == null)
        {
            LogHelper.LogWarrning($"No monster card drawn for rarity {rarity}");
            return;
        }

        monsterCardAccumulated.Add(drawn);
        LogHelper.Log($"👹 Monster card accumulated: [{drawn.rarity}] {drawn.displayName}");

        // 현재 살아있는 몬스터에도 즉시 적용
        ApplyMonsterCardToAllAlive(drawn);
    }

    void ApplyMonsterCardToAllAlive(MonsterCardData card)
    {
        List<MonsterBase> monsters = MonsterManager.Instance.GetAliveMonsters();
        foreach (var monster in monsters)
            ApplyMonsterCardToOne(monster, card);
    }

    public void ApplyMonsterCardToOne(MonsterBase monster, MonsterCardData card)
    {
        if (!card.IsStatCard) return;

        switch (card.statKey)
        {
            case "MaxHP":
                ApplyToNetVar(ref monster.maxHP, card.op, card.value);
                ApplyToNetVar(ref monster.currentHP, card.op, card.value);
                break;
            case "MoveSpeed":
                ApplyToNetVar(ref monster.moveSpeed, card.op, card.value);
                break;
            case "Armor":
                ApplyToNetVar(ref monster.defense, card.op, card.value);
                break;
            case "AttackDamage":
                ApplyToNetVar(ref monster.attackDamage, card.op, card.value);
                break;
            case "CountBase":
                // WaveManager의 GetMonsterCountMultiplier()에서 해석
                break;
            default:
                LogHelper.LogWarrning($"Monster card statKey not handled: {card.statKey}");
                break;
        }
    }

    void ApplyToNetVar(ref NetworkVariable<float> stat, CardOp op, float value)
    {
        stat.Value = op switch
        {
            CardOp.Add => stat.Value + value,
            CardOp.Mul => stat.Value * value,
            CardOp.Set => value,
            _ => stat.Value
        };
    }

    // ====================================================
    // 새 몬스터 스폰 시 → 누적된 몬스터 카드 전부 적용
    // MonsterBase.InitializeInternal 에서 호출
    // ====================================================

    public void ApplyAccumulatedMonsterCards(MonsterBase monster)
    {
        if (!IsServer) return;

        foreach (var card in monsterCardAccumulated)
            ApplyMonsterCardToOne(monster, card);
    }

    // ====================================================
    // 새 건물 건설 시 → 해당 플레이어의 누적 카드 전부 적용
    // BuildingManager.PlaceBuildingServerRpc 에서 호출
    // ====================================================

    public void ApplyAccumulatedPlayerCards(BuildingBase building, ulong ownerPlayerID)
    {
        if (!IsServer) return;
        if (building.GetCategory() != BuildingCategory.Tower) return;
        if (!playerCardStacks.ContainsKey(ownerPlayerID)) return;

        foreach (var kvp in playerCardStacks[ownerPlayerID])
        {
            string cardID = kvp.Key;
            int stackCount = kvp.Value;

            PlayerCardData card = CardDataManager.Instance.GetPlayerCard(cardID);
            if (card == null) continue;

            for (int i = 0; i < stackCount; i++)
            {
                if (card.IsStatCard)
                {
                    string modID = $"{cardID}_stack{i}";
                    string modName = $"{card.displayName} (Stack {i + 1})";
                    StatModifier modifier = CreateStatModifier(cardID, card.op, card.value, modID, modName);
                    if (modifier != null)
                        building.ApplyStatModifier(modifier);
                }
                else if (card.IsBehaviorCard && i == 0)
                {
                    IEventModifier eventMod = CreateBehaviorModifier(card.cardID, card.displayName, card.statKey, card.effectID);
                    if (eventMod != null)
                        building.ApplyEventModifier(eventMod);
                }
            }
        }
    }

    // ====================================================
    // WaveManager에서 스폰 수 계산 시 사용
    // 누적된 몬스터 카드의 CountBase 배율 반환
    // ====================================================

    public float GetMonsterCountMultiplier()
    {
        float multiplier = 1f;
        foreach (var card in monsterCardAccumulated)
        {
            if (card.statKey == "CountBase" && card.op == CardOp.Mul)
                multiplier *= card.value;
        }
        return multiplier;
    }

    // ====================================================
    // 공용 헬퍼
    // ====================================================

    StatModifier CreateStatModifier(string cardID, CardOp op, float value, string modID, string modName)
    {
        StatModifier.ModifierType modType = op switch
        {
            CardOp.Add => StatModifier.ModifierType.Additive,
            CardOp.Mul => StatModifier.ModifierType.Multiplicative,
            _ => StatModifier.ModifierType.Additive
        };

        if (op == CardOp.Set)
        {
            LogHelper.LogWarrning($"CardOp.Set not supported in StatModifier: {cardID}");
            return null;
        }

        return new StatModifier(modID, modName, modType, value);
    }

    IEventModifier CreateBehaviorModifier(string cardID, string cardName, string statKey, string effectID)
    {
        switch (statKey)
        {
            case "BurnDOT":
                LogHelper.Log($"🔥 BurnDOT Modifier (TODO): {effectID}");
                return null;

            case "ChainLightning":
                LogHelper.Log($"⚡ ChainLightning Modifier (TODO): {effectID}");
                return null;

            default:
                LogHelper.LogWarrning($"Unknown behavior statKey: {statKey}");
                return null;
        }
    }
}
