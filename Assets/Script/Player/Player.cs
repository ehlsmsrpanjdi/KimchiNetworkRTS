using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class Player : NetworkBehaviour
{
    [Header("Player Identity")]
    public NetworkVariable<ulong> PlayerID = new NetworkVariable<ulong>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [Header("Components")]
    public PlayerController controller;
    public PlayerResource resource;

    [Header("Building Ownership")]
    private Dictionary<BuildingCategory, List<BuildingBase>> ownedBuildings = new Dictionary<BuildingCategory, List<BuildingBase>>();

    private void Reset()
    {
        controller = GetComponent<PlayerController>();
        resource = GetComponent<PlayerResource>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
        {
            PlayerID.Value = OwnerClientId;
            PlayerManager.Instance.AddPlayer(OwnerClientId, this); // ✅ 등록
        }

        if (IsOwner)
        {
            // ✅ 로컬 플레이어 등록
            PlayerManager.Instance.SetLocalPlayer(this);

            // 내 플레이어 색상 변경 (테스트용)
            GetComponent<Renderer>().material.color = Color.blue;
        }
    }

    // ========== 건물 소유 관리 ==========
    public void RegisterBuilding(BuildingBase building)
    {
        if (!IsServer) return;

        BuildingCategory category = building.GetCategory(); // ✅ 수정

        if (!ownedBuildings.ContainsKey(category))
        {
            ownedBuildings[category] = new List<BuildingBase>();
        }

        ownedBuildings[category].Add(building);
    }

    public void UnregisterBuilding(BuildingBase building)
    {
        if (!IsServer) return;

        BuildingCategory type = building.GetCategory();

        if (ownedBuildings.ContainsKey(type))
        {
            ownedBuildings[type].Remove(building);
        }
    }

    public List<BuildingBase> GetBuildingsByType(BuildingCategory type)
    {
        if (ownedBuildings.ContainsKey(type))
        {
            return ownedBuildings[type];
        }
        return new List<BuildingBase>();
    }
}