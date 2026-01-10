using UnityEngine;
using System.Collections.Generic;

public class BuildingGhost : MonoBehaviour
{
    [Header("Building Info")]
    public int buildingID;
    private BuildingData data;

    [Header("Grid")]
    public GridArea grid;
    public LayerMask groundLayer;

    [Header("Visual")]
    public Color validColor = new Color(0, 1, 0, 0.5f);   // 초록 반투명
    public Color invalidColor = new Color(1, 0, 0, 0.5f); // 빨강 반투명

    [Header("Placement")]
    public float maxPlacementDistance = 10f; // 플레이어와 최대 거리

    private MeshRenderer meshRenderer;
    private Material ghostMaterial;
    private Vector2Int currentGridPos;
    private bool isValidPlacement;
    private Player ownerPlayer;

    // 충돌 체크용
    private List<GameObject> overlappingObjects = new List<GameObject>();

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        // Ghost 전용 머티리얼 생성 (반투명)
        if (meshRenderer != null)
        {
            ghostMaterial = new Material(meshRenderer.sharedMaterial);
            ghostMaterial.SetFloat("_Surface", 1); // Transparent
            ghostMaterial.SetFloat("_Blend", 0);   // Alpha
            ghostMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            ghostMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            ghostMaterial.SetInt("_ZWrite", 0);
            ghostMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            ghostMaterial.renderQueue = 3000;

            meshRenderer.material = ghostMaterial;
        }

        // Collider를 Trigger로 설정
        var collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }

    void Start()
    {
        grid = GridArea.Instance;
        groundLayer = LayerHelper.Instance.GetLayerToInt(LayerHelper.GridLayer);

        // BuildingData 로드
        data = BuildingDataManager.Instance.GetData(buildingID);
        if (data == null)
        {
            LogHelper.LogError($"BuildingData not found: {buildingID}");
            Destroy(gameObject);
            return;
        }

        // ✅ 로컬 플레이어 가져오기 (간단!)
        ownerPlayer = PlayerManager.Instance.GetLocalPlayer();
        if (ownerPlayer == null)
        {
            LogHelper.LogError("Local player not found!");
            Destroy(gameObject);
        }
    }

    // ✅ FindLocalPlayer() 메서드 삭제!

    void Update()
    {
        FollowMouse();
        UpdateVisual();
        CleanupOverlappingObjects();

        // 좌클릭 → 건물 설치
        if (Input.GetMouseButtonDown(0) && isValidPlacement)
        {
            PlaceBuilding();
        }

        // ESC → 취소
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelPlacement();
        }
    }

    void FollowMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
        {
            isValidPlacement = false;
            return;
        }

        // Grid 좌표로 변환
        Vector2Int gridPos = grid.WorldToGrid(hit.point);

        // Grid 범위 체크
        if (gridPos.x < 0 || gridPos.y < 0 ||
            gridPos.x + data.sizeX > grid.width ||
            gridPos.y + data.sizeY > grid.height)
        {
            isValidPlacement = false;
            transform.position = hit.point + Vector3.up * 0.5f;
            return;
        }

        // Grid에 스냅
        currentGridPos = gridPos;
        Vector3 snappedPos = grid.GridToWorldWithSize(gridPos.x, gridPos.y, data.sizeX, data.sizeY);
        transform.position = snappedPos + Vector3.up * 0.5f;

        // 유효성 체크
        CheckPlacementValidity();
    }

    void CheckPlacementValidity()
    {
        // 1) Grid 영역 사용 가능한지
        bool gridAvailable = grid.IsAreaAvailable(currentGridPos.x, currentGridPos.y, data.sizeX, data.sizeY);

        // 2) 플레이어와 거리 체크
        float distanceToPlayer = Vector3.Distance(transform.position, ownerPlayer.transform.position);
        bool withinRange = distanceToPlayer <= maxPlacementDistance;

        // 3) 다른 오브젝트와 겹치지 않는지
        bool noOverlap = overlappingObjects.Count == 0;

        // 4) 자원 충분한지 ✅ 여러 자원 체크
        bool hasResources = ownerPlayer.resource.HasEnoughResources(data.constructionCosts);

        isValidPlacement = gridAvailable && withinRange && noOverlap && hasResources;
    }

    void UpdateVisual()
    {
        if (ghostMaterial != null)
        {
            Color color = isValidPlacement ? validColor : invalidColor;
            ghostMaterial.color = color;
        }
    }

    void PlaceBuilding()
    {
        if (!isValidPlacement) return;

        // 자원 소모 ✅ 여러 자원 소모
        if (!ownerPlayer.resource.TrySpendResources(data.constructionCosts))
        {
            LogHelper.LogWarrning("자원 부족!");
            return;
        }

        // BuildingManager에 건물 설치 요청
        Vector3 worldPos = grid.GridToWorldWithSize(currentGridPos.x, currentGridPos.y, data.sizeX, data.sizeY);
        BuildingManager.Instance.PlaceBuildingServerRpc(
            buildingID,
            worldPos,
            currentGridPos,
            ownerPlayer.OwnerClientId
        );

        // Ghost 제거
        Destroy(gameObject);
    }

    void CancelPlacement()
    {
        LogHelper.Log("Building placement cancelled");
        Destroy(gameObject);
    }

    // ========== 충돌 감지 ==========
    private void OnTriggerEnter(Collider other)
    {
        // Building, Entity와 겹치는지 체크
        string layer = LayerHelper.Instance.GetObjectLayer(other.gameObject);
        if (layer == LayerHelper.BuildingLayer || layer == LayerHelper.EntityLayer)
        {
            overlappingObjects.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        overlappingObjects.Remove(other.gameObject);
    }

    void CleanupOverlappingObjects()
    {
        overlappingObjects.RemoveAll(obj => obj == null || !obj.activeSelf);
    }


    private void OnDestroy()
    {
        if (ghostMaterial != null)
        {
            Destroy(ghostMaterial);
        }
    }
}