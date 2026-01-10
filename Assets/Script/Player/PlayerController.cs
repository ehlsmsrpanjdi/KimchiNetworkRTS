using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : NetworkBehaviour
{
    [Header("Components")]
    public Camera playerCamera;
    public NavMeshAgent agent;

    [Header("Layers")]
    public LayerMask groundLayer;

    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
        groundLayer = LayerHelper.Instance.GetLayerToInt(LayerHelper.GridLayer);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsOwner)
        {
            // 다른 플레이어의 NavMeshAgent 비활성화
            if (agent != null)
                agent.enabled = false;
            return;
        }

        // 내 플레이어만 카메라 설정
        if (playerCamera == null)
        {
            playerCamera = FollowCamera.Instance.Camera;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        // 우클릭 이동
        if (Input.GetMouseButtonDown(1))
        {
            TryMoveToMouse();
        }
    }

    void TryMoveToMouse()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 999f, groundLayer))
        {
            agent.SetDestination(hit.point);
        }
    }
}