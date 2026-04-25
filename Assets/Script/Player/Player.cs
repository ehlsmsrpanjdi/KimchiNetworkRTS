using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class Player : NetworkBehaviour, ITakeDamage
{
    [Header("Player Identity")]
    public NetworkVariable<ulong> PlayerID = new NetworkVariable<ulong>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [Header("Stats")]
    public NetworkVariable<float> maxHP = new NetworkVariable<float>(100f,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<float> currentHP = new NetworkVariable<float>(100f,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> isDead = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Components")]
    [SerializeField] public PlayerController controller;
    [SerializeField] public PlayerResource resource;

    [Header("Building Ownership")]
    private Dictionary<BuildingCategory, List<BuildingBase>> ownedBuildings = new Dictionary<BuildingCategory, List<BuildingBase>>();

    [Header("Augments")]
    public NetworkList<int> ownedCardIDs;

    private void Reset()
    {
        controller = this.TryGetComponent<PlayerController>();
        resource = this.TryGetComponent<PlayerResource>();
    }

    private void Awake()
    {
        ownedCardIDs = new NetworkList<int>();

        var rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            PlayerID.Value = OwnerClientId;
            currentHP.Value = maxHP.Value;
            PlayerManager.Instance.AddPlayer(OwnerClientId, this);
        }

        if (IsOwner)
        {
            PlayerManager.Instance.SetLocalPlayer(this);
            GetComponent<Renderer>().material.color = Color.blue;
        }

        isDead.OnValueChanged += OnDeadStateChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        isDead.OnValueChanged -= OnDeadStateChanged;
    }

    // ========== ITakeDamage ==========
    public void TakeDamage(float damage)
    {
        if (!IsServer) return;
        if (isDead.Value) return;

        currentHP.Value = Mathf.Max(0f, currentHP.Value - damage);

        if (currentHP.Value <= 0f)
            OnDeath();
    }

    void OnDeath()
    {
        isDead.Value = true;
        LogHelper.Log($"💀 Player {OwnerClientId} died!");

        foreach (var buildings in ownedBuildings.Values)
            foreach (var building in buildings)
                building.SetDisabled(true);
    }

    void OnDeadStateChanged(bool prev, bool next)
    {
        if (!next) return;

        if (controller != null)
            controller.enabled = false;

        foreach (var rend in GetComponentsInChildren<Renderer>())
            rend.enabled = false;

        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    // ========== 건물 소유 관리 ==========
    public void RegisterBuilding(BuildingBase building)
    {
        if (!IsServer) return;

        BuildingCategory category = building.GetCategory();

        if (!ownedBuildings.ContainsKey(category))
            ownedBuildings[category] = new List<BuildingBase>();

        ownedBuildings[category].Add(building);
    }

    public void UnregisterBuilding(BuildingBase building)
    {
        if (!IsServer) return;

        BuildingCategory type = building.GetCategory();

        if (ownedBuildings.ContainsKey(type))
            ownedBuildings[type].Remove(building);
    }

    public List<BuildingBase> GetBuildingsByType(BuildingCategory type)
    {
        if (ownedBuildings.ContainsKey(type))
            return ownedBuildings[type];
        return new List<BuildingBase>();
    }
}
