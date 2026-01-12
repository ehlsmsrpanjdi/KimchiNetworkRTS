using Unity.Netcode;
using UnityEngine;

public class BuildingStat : NetworkBehaviour
{
    // ========== HP ==========
    public NetworkVariable<float> currentHP = new NetworkVariable<float>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<float> maxHP = new NetworkVariable<float>(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // ========== 방어력 ==========
    public NetworkVariable<float> defense = new NetworkVariable<float>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // ========== 공격 스탯 ==========
    public NetworkVariable<float> attackDamage = new NetworkVariable<float>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<float> attackSpeed = new NetworkVariable<float>(
        1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<float> attackRange = new NetworkVariable<float>(
        5,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<int> bulletCountPerAttack = new NetworkVariable<int>(
        1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // ========== BuildingData에서 초기화 ==========
    public void InitializeFromData(BuildingData data)
    {
        if (!IsServer) return;

        maxHP.Value = data.baseMaxHP;
        currentHP.Value = data.baseMaxHP;
        defense.Value = data.baseDefense;

        if (data.isAttackTower)
        {
            attackDamage.Value = data.baseAttackDamage;
            attackSpeed.Value = data.baseAttackSpeed;
            attackRange.Value = data.baseAttackRange;
        }

        // ✅ 자원 타워도 attackDamage, attackSpeed 사용
        if (data.category == BuildingCategory.Resource)
        {
            attackDamage.Value = data.baseAttackDamage;  // 최대 스택
            attackSpeed.Value = data.baseAttackSpeed;     // 초당 획득량
            attackRange.Value = data.baseAttackRange;     // 수확 범위
        }

        // ✅ 벽 타워
        if (data.category == BuildingCategory.Wall)
        {
            attackSpeed.Value = data.baseAttackSpeed;  // 초당 수리량
            attackRange.Value = data.baseAttackRange;  // 수리 범위
        }
    }

    // ========== Modifier 적용된 값 가져오기 ==========
    public float GetFinalAttackDamage(ModifierManager modifierManager)
    {
        return modifierManager.GetModifiedStat(attackDamage.Value);
    }

    public float GetFinalAttackSpeed(ModifierManager modifierManager)
    {
        return modifierManager.GetModifiedStat(attackSpeed.Value);
    }

    public float GetFinalAttackRange(ModifierManager modifierManager)
    {
        return modifierManager.GetModifiedStat(attackRange.Value);
    }
}