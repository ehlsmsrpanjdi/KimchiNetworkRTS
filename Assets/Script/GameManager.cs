using System;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class GameManager : NetworkBehaviour
{
    static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    public NetworkVariable<bool> isGameStart = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> isGameOver = new NetworkVariable<bool>(false,
        NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] Transform coreSpawnPoint;

    private void Reset()
    {
        coreSpawnPoint = transform.Find("SpawnPoint");
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    async void Start()
    {
        // Addressables 로딩
        await LoadManager.Instance.LoadTemp();
        ExcelDataLoader.Instance.LoadAll();

        Debug.Log("✅ Assets loaded!");
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerJoined;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnPlayerLeft;
        }
    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        //EntranceManager.Instance.SpawnRocks();
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    public void GameStart()
    {
        if (NetworkManager.Singleton.IsServer == false)
        {
            LogHelper.Log("Your Not Server or Not Open Server");
            return;
        }

        LogHelper.Log("GameStart");
        isGameStart.Value = true;
        isGameOver.Value = false;

        SpawnCore();
        WaveManager.Instance.StartWaveServerRpc();
    }

    void SpawnCore()
    {
        if (coreSpawnPoint == null)
        {
            LogHelper.LogError("GameManager/SpawnPoint 가 없습니다.");
            return;
        }

        var coreList = BuildingDataManager.Instance.GetByCategory(BuildingCategory.Core);
        BuildingData coreData = coreList.Count > 0 ? coreList[0] : null;
        if (coreData == null)
        {
            LogHelper.LogError("Core BuildingData가 없습니다.");
            return;
        }

        GameObject prefab = AssetManager.Instance.GetByName(coreData.prefabKey);
        if (prefab == null)
        {
            LogHelper.LogError($"Core prefab을 찾을 수 없습니다: {coreData.prefabKey}");
            return;
        }

        GameObject coreGo = Instantiate(prefab, coreSpawnPoint.position, Quaternion.identity);
        var netObj = coreGo.GetComponent<NetworkObject>();
        var building = coreGo.GetComponent<BuildingBase>();

        building.Initialize(coreData.buildingID, 0, Vector2Int.zero);
        netObj.Spawn();

        LogHelper.Log($"✅ Core spawned at {coreSpawnPoint.position}");
    }

    public void GameOver()
    {
        if (!IsServer) return;
        if (isGameOver.Value) return;

        LogHelper.Log("💥 GameOver!");
        isGameOver.Value = true;
        isGameStart.Value = false;
        WaveManager.Instance.isWaveActive.Value = false;

        GameOverClientRpc();
    }

    [Rpc(SendTo.Everyone)]
    void GameOverClientRpc()
    {
        LogHelper.Log("🔴 [Client] GameOver received!");
        // TODO: GameOver UI 표시
    }

    void OnPlayerJoined(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        LogHelper.Log($"Player joined: {clientId}");
    }

    void OnPlayerLeft(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        LogHelper.Log($"Player left: {clientId}");
    }


    void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayerJoined;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnPlayerLeft;
        }
    }
}