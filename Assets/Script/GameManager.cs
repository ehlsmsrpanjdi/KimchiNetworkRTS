using System;
using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    public bool isGameStart
    {
        get; private set;
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
        EntranceManager.Instance.SpawnRocks();
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

        else
        {
            LogHelper.Log("GameStart");
            isGameStart = true;
            WaveManager.Instance.StartWaveServerRpc();
        }
    }

    void OnPlayerJoined(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        LogHelper.Log($"Player joined: {clientId}");

        // 입구 업데이트
        EntranceManager.Instance?.UpdateEntranceWidth();
    }

    void OnPlayerLeft(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        LogHelper.Log($"Player left: {clientId}");

        // 입구 업데이트
        EntranceManager.Instance?.UpdateEntranceWidth();
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