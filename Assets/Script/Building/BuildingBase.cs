using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using System;

public class BuildingBase : NetworkBehaviour, ITakeDamage, IPoolObj
{
    [Header("Building Identity")]
    public int buildingID; // 건물 고유 ID

    [Header("Grid Reference")]
    public Vector2Int gridPosition;
    protected GridArea grid;

    [Header("Components")]
    public BuildingStat stat;
    protected BuildingData data;
    protected ModifierManager modifierManager;
    protected NavMeshObstacle navMeshObstacle;

    [Header("Owner")]
    public NetworkVariable<ulong> ownerPlayerID = new NetworkVariable<ulong>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // ========== 이벤트 ==========
    public event Action OnAttack;
    public event Action OnHit;
    public event Action OnDamaged;

    // ========== Unity Lifecycle ==========
    protected virtual void Awake()
    {
        modifierManager = new ModifierManager(this);
        grid = GridArea.Instance;
        stat = GetComponent<BuildingStat>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        stat.currentHP.OnValueChanged += OnHealthChanged;
    }

    protected virtual void Update()
    {
        if (!IsServer) return;
        modifierManager?.Update();
    }

    // ========== 초기화 (서버에서만 호출) ==========
    public void Initialize(int id, ulong ownerID, Vector2Int gridPos)
    {
        if (!IsServer) return;

        buildingID = id;
        ownerPlayerID.Value = ownerID;
        gridPosition = gridPos;

        // BuildingData 로드
        data = BuildingDataManager.Instance.GetData(id);
        if (data == null)
        {
            LogHelper.LogError($"BuildingData not found: {id}");
            return;
        }

        // Stat 초기화
        stat.InitializeFromData(data);

        // Grid 배치 (✅ data에서 크기 가져옴)
        SetupGrid();

        // NavMesh 설정
        SetupNavMeshObstacle();
    }

    // ========== Grid 설정 ==========
    void SetupGrid()
    {
        if (grid == null || data == null) return;

        // ✅ BuildingData의 크기 사용
        bool placed = grid.PlaceBuilding(gameObject, gridPosition.x, gridPosition.y, data.sizeX, data.sizeY);

        if (!placed)
        {
            LogHelper.LogError($"Failed to place building at {gridPosition}");
        }
    }

    // ========== NavMesh 설정 ==========
    void SetupNavMeshObstacle()
    {
        if (navMeshObstacle != null)
            Destroy(navMeshObstacle);

        navMeshObstacle = gameObject.AddComponent<NavMeshObstacle>();
        navMeshObstacle.carving = true;
        navMeshObstacle.carveOnlyStationary = true;
        navMeshObstacle.shape = NavMeshObstacleShape.Box;
        navMeshObstacle.center = Vector3.zero;

        // ✅ BuildingData의 크기에 맞춰 NavMesh 크기 설정
        if (data != null && grid != null)
        {
            navMeshObstacle.size = new Vector3(
                data.sizeX * grid.cellSize,
                2f,
                data.sizeY * grid.cellSize
            );
        }
        else
        {
            navMeshObstacle.size = Vector3.one;
        }
    }

    void RemoveNavMeshObstacle()
    {
        if (navMeshObstacle != null)
        {
            Destroy(navMeshObstacle);
            navMeshObstacle = null;
        }
    }

    // ========== Modifier 관리 ==========
    public void ApplyStatModifier(IStatModifier modifier)
    {
        modifierManager.AddStatModifier(modifier);
    }

    public void ApplyEventModifier(IEventModifier modifier)
    {
        modifierManager.AddEventModifier(modifier);
    }

    // ========== 이벤트 발동 ==========
    protected void TriggerOnAttack() => OnAttack?.Invoke();
    protected void TriggerOnHit() => OnHit?.Invoke();
    protected void TriggerOnDamaged() => OnDamaged?.Invoke();

    // ========== ITakeDamage 구현 ==========
    public virtual void TakeDamage(float damage)
    {
        if (!IsServer) return;

        // ✅ 방어력 적용 (고정 감소)
        float finalDamage = Mathf.Max(0, damage - stat.defense.Value);

        stat.currentHP.Value -= finalDamage;

        if (stat.currentHP.Value <= 0f)
        {
            stat.currentHP.Value = 0f;
            OnDeath();
        }

        TriggerOnDamaged();
    }

    protected virtual void OnDeath()
    {
        // 건물 파괴 (하위 클래스에서 구현)
    }

    void OnHealthChanged(float previousValue, float newValue)
    {
        // HP바 UI 업데이트 등
    }

    public BuildingCategory GetCategory()
    {
        if (data != null)
            return data.category;
        return BuildingCategory.Support; // 기본값
    }

    // ========== IPoolObj 구현 ==========
    public virtual void OnPush()
    {
        LogHelper.Log($"BuildingBase.OnPush: {gameObject.name}");

        // Grid에서 제거 (✅ data 크기 사용)
        if (grid != null && data != null)
        {
            grid.RemoveBuilding(gridPosition.x, gridPosition.y, data.sizeX, data.sizeY);
        }

        RemoveNavMeshObstacle();
        modifierManager.Clear();

        OnAttack = null;
        OnHit = null;
        OnDamaged = null;
    }

    public virtual void OnPop()
    {
        LogHelper.Log($"BuildingBase.OnPop: {gameObject.name}");

        if (IsServer)
        {
            stat.currentHP.Value = stat.maxHP.Value;
        }
    }
}