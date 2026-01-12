using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using System.Linq;

public enum AugmentTargetType
{
    All,           // 모든 건물
    AttackTower,   // 공격 타워만
    WallTower,     // 방어 타워만
    ResourceTower  // 자원 타워만
}

public class AugmentManager : NetworkBehaviour
{
    public static AugmentManager Instance;

    [Header("Rarity Config")]
    public AugmentRarityConfig rarityConfig = new AugmentRarityConfig();

    [Header("Current Wave Augments")]
    private List<AugmentData> currentOptions = new List<AugmentData>(3);

    // 플레이어별 획득한 증강 (augmentID → 획득 횟수)
    private Dictionary<ulong, Dictionary<int, int>> playerAugmentStacks = new Dictionary<ulong, Dictionary<int, int>>();

    // 몬스터 증강 (누적)
    private List<AugmentData> monsterAugments = new List<AugmentData>();

    public NetworkList<int> monsterAugmentIDs;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        monsterAugmentIDs = new NetworkList<int>();  // ✅ 초기화
    }

    // ========== 증강 선택 UI 표시 ==========
    [Rpc(SendTo.Server)]
    public void ShowAugmentSelectionServerRpc()
    {
        if (!IsServer) return;

        // 레어도 뽑기
        AugmentRarity rarity = rarityConfig.RollRarity();
        LogHelper.Log($"🎲 Rolled rarity: {rarity}");

        // 해당 레어도에서 3개 랜덤 선택
        List<AugmentData> pool = AugmentDataManager.Instance.GetByRarity(rarity);

        if (pool.Count < 3)
        {
            LogHelper.LogError($"Not enough augments for rarity {rarity}!");
            return;
        }

        // 랜덤 3개 선택 (중복 가능)
        currentOptions.Clear();
        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            currentOptions.Add(pool[randomIndex]);
        }

        // 클라이언트에 UI 표시 요청
        ShowAugmentUIClientRpc(
            currentOptions[0].augmentID,
            currentOptions[1].augmentID,
            currentOptions[2].augmentID
        );
    }

    [Rpc(SendTo.ClientsAndHost)]
    void ShowAugmentUIClientRpc(int augmentID1, int augmentID2, int augmentID3)
    {
        // TODO: UI 띄우기
        LogHelper.Log($"📋 Augment options: {augmentID1}, {augmentID2}, {augmentID3}");

        // 임시: Host(서버)만 자동 선택
        if (IsServer)
        {
            SelectAugmentServerRpc(0, NetworkManager.Singleton.LocalClientId);
        }
    }

    // ========== 증강 선택 ==========
    [Rpc(SendTo.Server)]
    public void SelectAugmentServerRpc(int optionIndex, ulong playerID)
    {
        if (!IsServer) return;
        if (optionIndex < 0 || optionIndex >= currentOptions.Count) return;

        AugmentData selectedAugment = currentOptions[optionIndex];
        LogHelper.Log($"✅ Player {playerID} selected: {selectedAugment.displayName}");

        // 플레이어에게 적용
        ApplyAugmentToPlayer(selectedAugment, playerID);

        // 몬스터에게 같은 레어도 랜덤 증강 적용
        ApplyAugmentToMonsters(selectedAugment.rarity);

        // 선택 완료
        currentOptions.Clear();
    }

    // ========== 플레이어 증강 적용 ==========
    void ApplyAugmentToPlayer(AugmentData augment, ulong playerID)
    {
        Player player = PlayerManager.Instance.GetPlayer(playerID);
        if (player == null)
        {
            LogHelper.LogError($"Player not found: {playerID}");
            return;
        }

        // ✅ NetworkList에 추가 (모든 클라가 볼 수 있음)
        player.ownedAugmentIDs.Add(augment.augmentID);

        // 스택 카운트 확인
        if (!playerAugmentStacks.ContainsKey(playerID))
        {
            playerAugmentStacks[playerID] = new Dictionary<int, int>();
        }

        int stackCount = playerAugmentStacks[playerID].GetValueOrDefault(augment.augmentID, 0);
        playerAugmentStacks[playerID][augment.augmentID] = stackCount + 1;

        bool isFirstTime = stackCount == 0;
        float value = isFirstTime ? augment.firstValue : augment.stackValue;

        // Stat 증강
        if (augment.IsStatAugment)
        {
            ApplyStatAugmentToPlayer(player, augment, value, stackCount);
        }
        // Event 증강
        else if (augment.IsEventAugment)
        {
            ApplyEventAugmentToPlayer(player, augment);
        }

        LogHelper.Log($"✅ Applied to player: {augment.displayName} (stack: {stackCount + 1})");
    }

    void ApplyStatAugmentToPlayer(Player player, AugmentData augment, float value, int stackCount)
    {
        // 플레이어의 모든 공격 건물에 적용
        List<BuildingBase> attackBuildings = player.GetBuildingsByType(BuildingCategory.Attack);

        string modifierID = $"{augment.augmentID}_stack{stackCount}";
        string modifierName = $"{augment.displayName} (Stack {stackCount + 1})";

        var statModifier = new StatModifier(modifierID, modifierName, augment.modifierType, value);

        foreach (var building in attackBuildings)
        {
            building.ApplyStatModifier(statModifier);
        }

        LogHelper.Log($"📈 Stat augment applied: {augment.targetStat} {(augment.modifierType == StatModifier.ModifierType.Additive ? "+" : "x")}{value}");
    }

    void ApplyEventAugmentToPlayer(Player player, AugmentData augment)
    {
        List<BuildingBase> attackBuildings = player.GetBuildingsByType(BuildingCategory.Attack);

        foreach (var building in attackBuildings)
        {
            IEventModifier eventModifier = CreateEventModifier(augment);
            if (eventModifier != null)
            {
                building.ApplyEventModifier(eventModifier);
            }
        }

        LogHelper.Log($"⚡ Event augment applied: {augment.eventType}");
    }

    IEventModifier CreateEventModifier(AugmentData augment)
    {
        string id = augment.augmentID.ToString();
        string name = augment.displayName;

        return augment.eventType switch
        {
            AugmentEventType.OnHit_AOE => new AOEModifier(id, name, augment.eventValue1, augment.eventValue2),
            AugmentEventType.OnAttack_DoubleBullet => new DoubleBulletModifier(id, name),
            AugmentEventType.OnHit_Debuff => new DebuffModifier(id, name, augment.eventValue1),
            _ => null
        };
    }

    // ========== 몬스터 증강 적용 ==========
    void ApplyAugmentToMonsters(AugmentRarity rarity)
    {
        List<AugmentData> pool = AugmentDataManager.Instance.GetByRarity(rarity);
        if (pool.Count == 0) return;

        AugmentData monsterAugment = pool[Random.Range(0, pool.Count)];
        monsterAugments.Add(monsterAugment);

        // ✅ NetworkList에 추가 (모든 클라가 볼 수 있음)
        monsterAugmentIDs.Add(monsterAugment.augmentID);

        LogHelper.Log($"👹 Monster augment added: {monsterAugment.displayName}");

        ApplyAugmentToAllMonsters(monsterAugment);
    }

    void ApplyAugmentToAllMonsters(AugmentData augment)
    {
        List<MonsterBase> monsters = MonsterManager.Instance.GetAliveMonsters();

        foreach (var monster in monsters)
        {
            ApplyAugmentToMonster(monster, augment);
        }
    }

    void ApplyAugmentToMonster(MonsterBase monster, AugmentData augment)
    {
        if (augment.IsStatAugment)
        {
            // 몬스터는 항상 firstValue 사용 (중복 스택 없음)
            float value = augment.firstValue;

            switch (augment.targetStat)
            {
                case AugmentTargetStat.AttackDamage:
                    if (augment.modifierType == StatModifier.ModifierType.Additive)
                        monster.attackDamage.Value += value;
                    else
                        monster.attackDamage.Value *= (1f + value);
                    break;

                case AugmentTargetStat.AttackSpeed:
                    if (augment.modifierType == StatModifier.ModifierType.Additive)
                        monster.attackSpeed.Value += value;
                    else
                        monster.attackSpeed.Value *= (1f + value);
                    break;

                case AugmentTargetStat.AttackRange:
                    if (augment.modifierType == StatModifier.ModifierType.Additive)
                        monster.attackRange.Value += value;
                    else
                        monster.attackRange.Value *= (1f + value);
                    break;
            }
        }
        // 몬스터는 Event 증강 없음 (필요하면 추가)
    }

    // ========== 새 몬스터 스폰 시 증강 적용 ==========
    public void ApplyAugmentsToNewMonster(MonsterBase monster)
    {
        if (!IsServer) return;

        foreach (var augment in monsterAugments)
        {
            ApplyAugmentToMonster(monster, augment);
        }
    }

    // ========== 새 건물 생성 시 증강 적용 ==========
    public void ApplyAugmentsToNewBuilding(BuildingBase building, ulong ownerPlayerID)
    {
        if (!IsServer) return;
        if (building.GetCategory() != BuildingCategory.Attack) return;

        if (!playerAugmentStacks.ContainsKey(ownerPlayerID)) return;

        foreach (var kvp in playerAugmentStacks[ownerPlayerID])
        {
            int augmentID = kvp.Key;
            int stackCount = kvp.Value;

            AugmentData augment = AugmentDataManager.Instance.GetData(augmentID);
            if (augment == null) continue;

            // ✅ 여기에 추가
            if (augment.eventType == AugmentEventType.OnAttack_DoubleBullet)
            {
                building.stat.bulletCountPerAttack.Value += stackCount;
                LogHelper.Log($"✅ BulletCount set: {building.stat.bulletCountPerAttack.Value}");
                continue;
            }

            // 각 스택마다 적용
            for (int i = 0; i < stackCount; i++)
            {
                bool isFirst = i == 0;
                float value = isFirst ? augment.firstValue : augment.stackValue;

                if (augment.IsStatAugment)
                {
                    string modID = $"{augmentID}_stack{i}";
                    string modName = $"{augment.displayName} (Stack {i + 1})";
                    var modifier = new StatModifier(modID, modName, augment.modifierType, value);
                    building.ApplyStatModifier(modifier);
                }
                else if (augment.IsEventAugment)
                {
                    if (i == 0) // Event는 1번만
                    {
                        var eventMod = CreateEventModifier(augment);
                        if (eventMod != null)
                        {
                            building.ApplyEventModifier(eventMod);
                        }
                    }
                }
            }
        }
    }
}