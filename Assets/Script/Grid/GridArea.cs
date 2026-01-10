using Unity.AI.Navigation;
using UnityEngine;

[ExecuteAlways]
public class GridArea : MonoBehaviour
{
    public static GridArea Instance;
    [Header("Grid Size (cells)")]
    public int width = 10;
    public int height = 10;

    [Header("Cell Size")]
    public float cellSize = 1f;

    [Header("Visual")]
    public Color gridColor = Color.green;
    public bool drawGrid = true;

    [Header("Navigation")]
    public bool useNavMesh = true;

    // 그리드 점유 상태 저장
    private GameObject[,] occupiedCells;

    private void Awake()
    {
        occupiedCells = new GameObject[width, height];
        Instance = this;
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        float halfW = (width * cellSize) * 0.5f;
        float halfH = (height * cellSize) * 0.5f;
        Vector3 origin = transform.position - new Vector3(halfW, 0, halfH);

        int x = Mathf.FloorToInt((worldPos.x - origin.x) / cellSize);
        int y = Mathf.FloorToInt((worldPos.z - origin.z) / cellSize);

        return new Vector2Int(x, y);
    }

    public Vector3 GridToWorld(int x, int y)
    {
        float halfW = (width * cellSize) * 0.5f;
        float halfH = (height * cellSize) * 0.5f;
        Vector3 origin = transform.position - new Vector3(halfW, 0, halfH);

        return origin + new Vector3((x + 0.5f) * cellSize, 0, (y + 0.5f) * cellSize);
    }

    public Vector3 GridToWorldWithSize(int x, int y, int sizeX, int sizeY)
    {
        float halfW = (width * cellSize) * 0.5f;
        float halfH = (height * cellSize) * 0.5f;
        Vector3 origin = transform.position - new Vector3(halfW, 0, halfH);

        float centerOffsetX = (sizeX * cellSize) * 0.5f;
        float centerOffsetZ = (sizeY * cellSize) * 0.5f;

        return origin + new Vector3(x * cellSize + centerOffsetX, 0, y * cellSize + centerOffsetZ);
    }

    public bool IsAreaAvailable(int startX, int startY, int sizeX, int sizeY)
    {
        if (startX < 0 || startY < 0 ||
            startX + sizeX > width || startY + sizeY > height)
        {
            return false;
        }

        for (int x = startX; x < startX + sizeX; x++)
        {
            for (int y = startY; y < startY + sizeY; y++)
            {
                if (occupiedCells[x, y] != null)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public bool PlaceBuilding(GameObject building, int startX, int startY, int sizeX, int sizeY)
    {
        if (!IsAreaAvailable(startX, startY, sizeX, sizeY))
        {
            return false;
        }

        for (int x = startX; x < startX + sizeX; x++)
        {
            for (int y = startY; y < startY + sizeY; y++)
            {
                occupiedCells[x, y] = building;
            }
        }

        return true;
    }

    public void RemoveBuilding(int startX, int startY, int sizeX, int sizeY)
    {
        for (int x = startX; x < startX + sizeX; x++)
        {
            for (int y = startY; y < startY + sizeY; y++)
            {
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    occupiedCells[x, y] = null;
                }
            }
        }
    }

    // NavMesh 재계산 (선택사항 - Runtime NavMesh Baking이 필요함)
    public void UpdateNavMesh()
    {
        if (!useNavMesh) return;

        // NavMeshSurface가 있으면 다시 Bake
        NavMeshSurface surface = GetComponent<NavMeshSurface>();
        if (surface != null)
        {
            surface.BuildNavMesh();
            Debug.Log("NavMesh rebuilt for GridArea");
        }
    }

    private void OnDrawGizmos()
    {
        if (!drawGrid) return;

        Gizmos.color = gridColor;

        float halfW = (width * cellSize) * 0.5f;
        float halfH = (height * cellSize) * 0.5f;
        Vector3 origin = transform.position - new Vector3(halfW, 0, halfH);

        // 세로 라인
        for (int x = 0; x <= width; x++)
        {
            Vector3 from = origin + new Vector3(x * cellSize, 0, 0);
            Vector3 to = from + new Vector3(0, 0, height * cellSize);
            Gizmos.DrawLine(from, to);
        }

        // 가로 라인
        for (int y = 0; y <= height; y++)
        {
            Vector3 from = origin + new Vector3(0, 0, y * cellSize);
            Vector3 to = from + new Vector3(width * cellSize, 0, 0);
            Gizmos.DrawLine(from, to);
        }
    }
}