using UnityEngine;
using Unity.Netcode;

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

    }

    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
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

}