using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using System;

public class BuildingBase : NetworkBehaviour, ITakeDamage, IPoolObj
{
    [Header("Building Identity")]
    public int buildingID;

    [Header("Grid Reference")]
    public Vector2Int gridPosition;
    protected GridArea grid;

    [Header("Components")]
    public BuildingStat stat;
    protected BuildingData data;
    protected ModifierManager modifierManager;
    protected NavMeshObstacle navMeshObstacle;
    protected SphereCollider proximityCollider;  // ✅ 추가

    [Header("Owner")]
    public NetworkVariable<ulong> ownerPlayerID = new NetworkVariable<ulong>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // ========== 이벤트 ==========
    public event Action OnAttack;
    public event Action<MonsterBase> OnHit;
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

    // ========== 초기화 ==========
    public void Initialize(int id, ulong ownerID, Vector2Int gridPos)
    {
        buildingID = id;
        ownerPlayerID.Value = ownerID;
        gridPosition = gridPos;

        data = BuildingDataManager.Instance.GetData(id);
        if (data == null)
        {
            LogHelper.LogError($"BuildingData not found: {id}");
            return;
        }

        stat.InitializeFromData(data);
        SetupGrid();
        SetupNavMeshObstacle();
        SetupProximityCollider();  // ✅ 추가

        OnInitialized();
    }

    protected virtual void OnInitialized()
    {
        // 하위 클래스에서 구현
    }

    // ========== Proximity Collider 설정 ==========
    void SetupProximityCollider()
    {
        // 기존 SphereCollider 찾기
        proximityCollider = GetComponent<SphereCollider>();

        if (proximityCollider == null)
        {
            proximityCollider = gameObject.AddComponent<SphereCollider>();
        }

        proximityCollider.isTrigger = true;
        proximityCollider.radius = stat.attackRange.Value;
        proximityCollider.center = Vector3.zero;

        LogHelper.Log($"✅ Proximity collider setup: radius = {proximityCollider.radius}");
    }

    void RemoveProximityCollider()
    {
        if (proximityCollider != null)
        {
            Destroy(proximityCollider);
            proximityCollider = null;
        }
    }

    // ========== Trigger 이벤트 ==========
    void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (LayerHelper.Instance.GetObjectLayer(other.gameObject) == LayerHelper.PlayerLayer)
        {
            var player = other.GetComponent<Player>();
            if (player != null && player.OwnerClientId == ownerPlayerID.Value)
            {
                OnPlayerEnterRange(player);
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!IsServer) return;

        if (LayerHelper.Instance.GetObjectLayer(other.gameObject) == LayerHelper.PlayerLayer)
        {
            var player = other.GetComponent<Player>();
            if (player != null && player.OwnerClientId == ownerPlayerID.Value)
            {
                OnPlayerStayRange(player);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;

        if (LayerHelper.Instance.GetObjectLayer(other.gameObject) == LayerHelper.PlayerLayer)
        {
            var player = other.GetComponent<Player>();
            if (player != null && player.OwnerClientId == ownerPlayerID.Value)
            {
                OnPlayerExitRange(player);
            }
        }
    }

    // ✅ 하위 클래스에서 오버라이드
    protected virtual void OnPlayerEnterRange(Player player) { }
    protected virtual void OnPlayerStayRange(Player player) { }
    protected virtual void OnPlayerExitRange(Player player) { }

    // ========== Grid 설정 ==========
    void SetupGrid()
    {
        if (grid == null || data == null) return;

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
    public void TriggerOnAttack() => OnAttack?.Invoke();
    public void TriggerOnHit(MonsterBase target) => OnHit?.Invoke(target);
    public void TriggerOnDamaged() => OnDamaged?.Invoke();

    // ========== ITakeDamage 구현 ==========
    public virtual void TakeDamage(float damage)
    {
        if (!IsServer) return;

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
        // 하위 클래스에서 구현
    }

    void OnHealthChanged(float previousValue, float newValue)
    {
        // HP바 UI 업데이트 등
    }

    public BuildingCategory GetCategory()
    {
        if (data != null)
            return data.category;
        return BuildingCategory.Support;
    }

    // ========== IPoolObj 구현 ==========
    public virtual void OnPush()
    {
        LogHelper.Log($"BuildingBase.OnPush: {gameObject.name}");

        if (grid != null && data != null)
        {
            grid.RemoveBuilding(gridPosition.x, gridPosition.y, data.sizeX, data.sizeY);
        }

        RemoveNavMeshObstacle();
        RemoveProximityCollider();  // ✅ 추가
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