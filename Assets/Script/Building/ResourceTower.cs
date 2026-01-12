using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 자원 생산 건물 (플레이어 근처에서 수확)
/// </summary>
public class ResourceTower : BuildingBase
{
    [Header("Resource Settings")]
    private ResourceType resourceType;
    private float maxStack;
    private float stackGainRate;
    private float harvestDuration;

    [Header("Current State")]
    public NetworkVariable<float> currentStack = new NetworkVariable<float>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private Dictionary<ulong, float> harvestProgress = new Dictionary<ulong, float>();
    private bool isInitialized = false;

    protected override void Update()
    {
        base.Update();

        if (!IsServer) return;
        if (!isInitialized) return;

        GainResource();
    }

    void GainResource()
    {
        if (currentStack.Value >= maxStack)
            return;

        currentStack.Value = Mathf.Min(currentStack.Value + stackGainRate * Time.deltaTime, maxStack);
    }

    // ========== 플레이어 근접 효과 ==========
    protected override void OnPlayerStayRange(Player player)
    {
        if (currentStack.Value <= 0) return;

        if (!harvestProgress.ContainsKey(player.OwnerClientId))
        {
            harvestProgress[player.OwnerClientId] = 0f;
        }

        harvestProgress[player.OwnerClientId] += Time.deltaTime;

        if (harvestProgress[player.OwnerClientId] >= harvestDuration)
        {
            HarvestResource(player);
            harvestProgress[player.OwnerClientId] = 0f;
        }
    }

    protected override void OnPlayerExitRange(Player player)
    {
        harvestProgress.Remove(player.OwnerClientId);
    }

    void HarvestResource(Player player)
    {
        if (currentStack.Value <= 0) return;

        int amount = Mathf.FloorToInt(currentStack.Value);

        player.resource.AddResource(resourceType, amount);
        currentStack.Value = 0f;

        LogHelper.Log($"🌾 Player {player.OwnerClientId} harvested {amount} {resourceType}");
    }

    // ========== 초기화 ==========
    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (data != null && data.category == BuildingCategory.Resource)
        {
            resourceType = data.resourceType;
            maxStack = data.baseAttackDamage;
            stackGainRate = data.baseAttackSpeed;
            harvestDuration = data.harvestDuration;

            isInitialized = true;
            LogHelper.Log($"✅ ResourceTower initialized: {data.displayName}");
        }
    }

    public override void OnPop()
    {
        base.OnPop();
        isInitialized = false;
        harvestProgress.Clear();
        currentStack.Value = 0f;
    }

    public override void OnPush()
    {
        base.OnPush();
        isInitialized = false;
        harvestProgress.Clear();
        currentStack.Value = 0f;
    }

    // ========== 디버그 ==========
    void OnDrawGizmosSelected()
    {
        if (data == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stat.attackRange.Value);
    }
}