using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkPoolManager : MonoBehaviour
{
    public static NetworkPoolManager Instance;

    // Prefab(원본) → Queue<실제 인스턴스>
    private Dictionary<GameObject, Queue<NetworkObject>> pools = new();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted += RegisterAllHandlers;
            NetworkManager.Singleton.OnClientStarted += RegisterAllHandlers;
        }
    }

    void RegisterAllHandlers()
    {
        RegisterByLabel(ResourceString.LabelBullet);
        RegisterByLabel(ResourceString.LabelBuilding);
        RegisterByLabel(ResourceString.LabelMonster); 
    }

    void RegisterByLabel(string label)
    {
        var prefabs = AssetManager.Instance.GetPrefabsByLabel(label);
        foreach (var prefab in prefabs)
        {
            if (prefab.GetComponent<NetworkObject>() != null)
            {
                RegisterPrefab(prefab);
            }
        }
    }

    void RegisterPrefab(GameObject prefab)
    {
        if (pools.ContainsKey(prefab))
            return;

        var handler = new PooledPrefabHandler(prefab, this);
        NetworkManager.Singleton.PrefabHandler.AddHandler(prefab, handler);

        pools[prefab] = new Queue<NetworkObject>();

        LogHelper.Log($"✅ Handler 등록: {prefab.name}");
    }

    // ★★★ 서버/클라이언트 모두 호출 ★★★
    public NetworkObject GetFromPool(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        if (!pools.ContainsKey(prefab))
            pools[prefab] = new Queue<NetworkObject>();

        NetworkObject obj;

        if (pools[prefab].Count > 0)
        {
            obj = pools[prefab].Dequeue();
            obj.transform.SetPositionAndRotation(pos, rot);
            obj.gameObject.SetActive(true);
            obj.GetComponent<IPoolObj>()?.OnPop();

            LogHelper.Log($"🔵 Pool에서 꺼냄: {prefab.name}");
        }
        else
        {
            var instance = Instantiate(prefab, pos, rot);
            instance.name = prefab.name;
            obj = instance.GetComponent<NetworkObject>();
            obj.GetComponent<IPoolObj>()?.OnPop();

            LogHelper.Log($"🟢 새로 생성: {prefab.name}");
        }

        return obj;
    }

    // ★★★ 서버/클라이언트 모두 호출 ★★★
    public void ReturnToPool(GameObject prefab, NetworkObject obj)
    {
        if (!pools.ContainsKey(prefab))
            pools[prefab] = new Queue<NetworkObject>();

        obj.GetComponent<IPoolObj>()?.OnPush();
        obj.gameObject.SetActive(false);
        pools[prefab].Enqueue(obj);

        LogHelper.Log($"🔴 Pool에 반환: {prefab.name}");
    }
}

public class PooledPrefabHandler : INetworkPrefabInstanceHandler
{
    GameObject prefab;
    NetworkPoolManager poolManager;

    public PooledPrefabHandler(GameObject prefab, NetworkPoolManager manager)
    {
        this.prefab = prefab;
        this.poolManager = manager;
    }

    // ★★★ 클라이언트에서만 호출 ★★★
    public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        LogHelper.Log($"🔵 Handler.Instantiate (클라이언트): {prefab.name}");
        return poolManager.GetFromPool(prefab, position, rotation);
    }

    public void Destroy(NetworkObject networkObject)
    {
        LogHelper.Log($"🔴 Handler.Destroy 호출: {prefab.name}, GameObject: {networkObject.gameObject.name}");
        poolManager.ReturnToPool(prefab, networkObject);
    }
}