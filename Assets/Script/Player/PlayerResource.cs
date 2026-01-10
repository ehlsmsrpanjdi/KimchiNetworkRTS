using System;
using Unity.Netcode;

public class PlayerResource : NetworkBehaviour
{
    // ========== 각 자원별 NetworkVariable ==========
    public NetworkVariable<int> Gold = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> Wood = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> Stone = new NetworkVariable<int>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // ========== 이벤트 ==========
    public Action<ResourceType, int> OnResourceChanged;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // 각 자원 변경 이벤트 등록
        Gold.OnValueChanged += (prev, next) => OnResourceValueChanged(ResourceType.Gold, next);
        Wood.OnValueChanged += (prev, next) => OnResourceValueChanged(ResourceType.Wood, next);
        Stone.OnValueChanged += (prev, next) => OnResourceValueChanged(ResourceType.Stone, next);

        // 초기값 UI 업데이트
        if (IsOwner)
        {
            OnResourceChanged?.Invoke(ResourceType.Gold, Gold.Value);
            OnResourceChanged?.Invoke(ResourceType.Wood, Wood.Value);
            OnResourceChanged?.Invoke(ResourceType.Stone, Stone.Value);
        }
    }

    void OnResourceValueChanged(ResourceType type, int newValue)
    {
        if (IsOwner)
        {
            OnResourceChanged?.Invoke(type, newValue);
        }
    }

    // ========== 단일 자원 확인 ==========
    public bool HasEnoughResource(ResourceType type, int amount)
    {
        return GetResource(type) >= amount;
    }

    // ✅ 여러 자원 확인
    public bool HasEnoughResources(ResourceCost[] costs)
    {
        if (costs == null || costs.Length == 0)
            return true;

        foreach (var cost in costs)
        {
            if (!HasEnoughResource(cost.resourceType, cost.amount))
            {
                return false;
            }
        }
        return true;
    }

    public int GetResource(ResourceType type)
    {
        return type switch
        {
            ResourceType.Gold => Gold.Value,
            ResourceType.Wood => Wood.Value,
            ResourceType.Stone => Stone.Value,
            _ => 0
        };
    }

    // ========== 단일 자원 사용 ==========
    public bool TrySpendResource(ResourceType type, int amount)
    {
        if (!IsOwner) return false;

        if (!HasEnoughResource(type, amount))
        {
            LogHelper.LogWarrning($"{type} 부족! 필요: {amount}, 보유: {GetResource(type)}");
            return false;
        }

        SpendResourceServerRpc(type, amount);
        return true;
    }

    // ✅ 여러 자원 사용
    public bool TrySpendResources(ResourceCost[] costs)
    {
        if (!IsOwner) return false;

        if (costs == null || costs.Length == 0)
            return true;

        // 로컬 체크
        if (!HasEnoughResources(costs))
        {
            LogHelper.LogWarrning("자원 부족!");
            return false;
        }

        // Server에 요청
        SpendResourcesServerRpc(costs);
        return true;
    }

    [Rpc(SendTo.Server)]
    void SpendResourceServerRpc(ResourceType type, int amount)
    {
        if (!HasEnoughResource(type, amount))
        {
            LogHelper.LogWarrning("치팅 시도 감지!");
            return;
        }

        switch (type)
        {
            case ResourceType.Gold:
                Gold.Value -= amount;
                break;
            case ResourceType.Wood:
                Wood.Value -= amount;
                break;
            case ResourceType.Stone:
                Stone.Value -= amount;
                break;
        }
    }

    [Rpc(SendTo.Server)]
    void SpendResourcesServerRpc(ResourceCost[] costs)
    {
        // Server에서 재검증
        if (!HasEnoughResources(costs))
        {
            LogHelper.LogWarrning("치팅 시도 감지!");
            return;
        }

        foreach (var cost in costs)
        {
            switch (cost.resourceType)
            {
                case ResourceType.Gold:
                    Gold.Value -= cost.amount;
                    break;
                case ResourceType.Wood:
                    Wood.Value -= cost.amount;
                    break;
                case ResourceType.Stone:
                    Stone.Value -= cost.amount;
                    break;
            }
        }
    }

    // ========== 자원 추가 ==========
    public void AddResource(ResourceType type, int amount)
    {
        if (!IsOwner) return;
        AddResourceServerRpc(type, amount);
    }

    [Rpc(SendTo.Server)]
    void AddResourceServerRpc(ResourceType type, int amount)
    {
        switch (type)
        {
            case ResourceType.Gold:
                Gold.Value += amount;
                break;
            case ResourceType.Wood:
                Wood.Value += amount;
                break;
            case ResourceType.Stone:
                Stone.Value += amount;
                break;
        }
    }
}