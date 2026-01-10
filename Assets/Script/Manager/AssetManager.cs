using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AssetManager
{
    static AssetManager instance;
    public static AssetManager Instance
    {
        get
        {
            if (instance == null)
                instance = new AssetManager();
            return instance;
        }
    }

    // 라벨 → 로드된 모든 프리팹
    private Dictionary<string, List<GameObject>> labelToPrefabs = new();

    // 이름 → 프리팹
    private Dictionary<string, GameObject> nameToPrefab = new();

    // 타입 → 프리팹
    private Dictionary<Type, GameObject> typeToPrefab = new();

    // 라벨 → 로드 핸들들(릴리즈용)
    private Dictionary<string, List<AsyncOperationHandle>> labelToHandles = new();

    // ----------------------------
    // 기존 LoadByLabel (비동기지만 Task 반환 없음)
    // ----------------------------
    public AsyncOperationHandle LoadByLabel(string label)
    {
        var handle = Addressables.LoadAssetsAsync<GameObject>(label, prefab =>
        {
            CachePrefab(label, prefab);
        });

        if (!labelToHandles.ContainsKey(label))
            labelToHandles[label] = new List<AsyncOperationHandle>();

        labelToHandles[label].Add(handle);

        return handle;
    }

    // ----------------------------
    //  Task 반환하는 LoadByLabelAsync (새로운 함수)
    // ----------------------------
    public async Task LoadByLabelAsync(string label)
    {
        var handle = Addressables.LoadAssetsAsync<GameObject>(label, prefab =>
        {
            CachePrefab(label, prefab);
        });

        if (!labelToHandles.ContainsKey(label))
            labelToHandles[label] = new List<AsyncOperationHandle>();

        labelToHandles[label].Add(handle);

        // 로딩 완료까지 정확히 대기
        await handle.Task;
    }

    // ----------------------------
    //  프리팹 캐싱 로직 (중복 제거)
    // ----------------------------
    private void CachePrefab(string label, GameObject prefab)
    {
        if (!labelToPrefabs.ContainsKey(label))
            labelToPrefabs[label] = new List<GameObject>();

        labelToPrefabs[label].Add(prefab);
        nameToPrefab[prefab.name] = prefab;

        var comp = prefab.GetComponent<MonoBehaviour>();
        if (comp != null)
        {
            typeToPrefab[comp.GetType()] = prefab;
        }
    }

    // ----------------------------
    //  라벨 단위 릴리즈
    // ----------------------------
    public void ReleaseByLabel(string label)
    {
        if (!labelToHandles.ContainsKey(label))
            return;

        foreach (var h in labelToHandles[label])
            Addressables.Release(h);

        labelToHandles.Remove(label);

        if (labelToPrefabs.ContainsKey(label))
            labelToPrefabs.Remove(label);

        nameToPrefab.Clear();
        typeToPrefab.Clear();
    }

    // ----------------------------
    //  이름으로 프리팹 가져오기
    // ----------------------------
    public GameObject GetByName(string name)
    {
        return nameToPrefab.TryGetValue(name, out var prefab) ? prefab : null;
    }

    // ----------------------------
    //  타입으로 프리팹 가져오기
    // ----------------------------
    public GameObject GetByType<T>() where T : MonoBehaviour
    {
        return typeToPrefab.TryGetValue(typeof(T), out var prefab) ? prefab : null;
    }

    public GameObject GetByType(Type type)
    {
        return typeToPrefab.TryGetValue(type, out var prefab) ? prefab : null;
    }

    public List<GameObject> GetPrefabsByLabel(string label)
    {
        return labelToPrefabs.TryGetValue(label, out var list) ? list : new List<GameObject>();
    }

    public GameObject Spawn(string name)
    {
        if (true == nameToPrefab.TryGetValue(name, out var prefab))
        {
            return GameObject.Instantiate(prefab);
        }
        else
        {
            return null;
        }
    }
}
