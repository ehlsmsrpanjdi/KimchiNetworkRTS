using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 방어 건물 (플레이어 근처에서 자동 수리)
/// </summary>
public class WallTower : BuildingBase
{
    private bool isInitialized = false;

    protected override void Update()
    {
        base.Update();

        if (!IsServer) return;
        if (!isInitialized) return;
    }

    // ========== 플레이어 근접 효과 ==========
    protected override void OnPlayerStayRange(Player player)
    {
        // 이미 풀피면 수리 안 함
        if (stat.currentHP.Value >= stat.maxHP.Value)
            return;

        float repairAmount = stat.attackSpeed.Value * Time.deltaTime;
        stat.currentHP.Value = Mathf.Min(stat.currentHP.Value + repairAmount, stat.maxHP.Value);
    }

    // ========== 초기화 ==========
    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (data != null && data.category == BuildingCategory.Wall)
        {
            isInitialized = true;
            LogHelper.Log($"✅ WallTower initialized: {data.displayName}");
        }
    }

    public override void OnPop()
    {
        base.OnPop();
        isInitialized = false;
    }

    public override void OnPush()
    {
        base.OnPush();
        isInitialized = false;
    }

    // ========== 디버그 ==========
    void OnDrawGizmosSelected()
    {
        if (data == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stat.attackRange.Value);
    }
}