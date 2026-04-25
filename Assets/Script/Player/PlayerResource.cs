using System;
using Unity.Netcode;

public class PlayerResource : NetworkBehaviour
{
    public NetworkVariable<int> Iron = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public Action<int> OnResourceChanged;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Iron.OnValueChanged += (prev, next) => OnResourceChanged?.Invoke(next);

        if (IsOwner)
            OnResourceChanged?.Invoke(Iron.Value);
    }

    public bool HasEnoughResource(int amount) => Iron.Value >= amount;

    public bool HasEnoughResources(ResourceCost[] costs)
    {
        if (costs == null || costs.Length == 0) return true;
        int total = 0;
        foreach (var cost in costs) total += cost.amount;
        return Iron.Value >= total;
    }

    public int GetResource() => Iron.Value;

    public bool TrySpendResource(int amount)
    {
        if (!IsOwner) return false;
        if (!HasEnoughResource(amount))
        {
            LogHelper.LogWarrning($"Iron 부족! 필요: {amount}, 보유: {Iron.Value}");
            return false;
        }
        SpendResourceServerRpc(amount);
        return true;
    }

    public bool TrySpendResources(ResourceCost[] costs)
    {
        if (!IsOwner) return false;
        if (!HasEnoughResources(costs))
        {
            LogHelper.LogWarrning("자원 부족!");
            return false;
        }
        int total = 0;
        foreach (var cost in costs) total += cost.amount;
        SpendResourceServerRpc(total);
        return true;
    }

    [Rpc(SendTo.Server)]
    void SpendResourceServerRpc(int amount)
    {
        if (Iron.Value < amount) { LogHelper.LogWarrning("치팅 시도 감지!"); return; }
        Iron.Value -= amount;
    }

    public void AddResource(int amount)
    {
        if (IsServer) { Iron.Value += amount; return; }
        if (!IsOwner) return;
        AddResourceServerRpc(amount);
    }

    [Rpc(SendTo.Server)]
    void AddResourceServerRpc(int amount) => Iron.Value += amount;
}
