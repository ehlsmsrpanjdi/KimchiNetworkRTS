using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    // effectName → Queue<EffectBase>
    private Dictionary<string, Queue<EffectBase>> pools = new Dictionary<string, Queue<EffectBase>>();

    // effectName → Prefab
    private Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

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
        LoadEffects();
    }

    void LoadEffects()
    {
        // Addressables에서 "Effect" 라벨로 로드
        var effectPrefabs = AssetManager.Instance.GetPrefabsByLabel("Effect");

        foreach (var prefab in effectPrefabs)
        {
            var effect = prefab.GetComponent<EffectBase>();
            if (effect != null)
            {
                prefabs[effect.effectName] = prefab;
                pools[effect.effectName] = new Queue<EffectBase>();
                LogHelper.Log($"✅ Effect registered: {effect.effectName}");
            }
        }
    }

    /// <summary>
    /// 이펙트 재생
    /// </summary>
    public void Play(string effectName, Vector3 position, Quaternion rotation = default)
    {
        if (rotation == default)
            rotation = Quaternion.identity;

        EffectBase effect = GetFromPool(effectName);
        if (effect != null)
        {
            effect.Play(position, rotation);
        }
    }

    /// <summary>
    /// 풀에서 꺼내기
    /// </summary>
    EffectBase GetFromPool(string effectName)
    {
        if (!prefabs.ContainsKey(effectName))
        {
            LogHelper.LogError($"Effect prefab not found: {effectName}");
            return null;
        }

        if (!pools.ContainsKey(effectName))
        {
            pools[effectName] = new Queue<EffectBase>();
        }

        EffectBase effect;

        if (pools[effectName].Count > 0)
        {
            effect = pools[effectName].Dequeue();
            effect.OnPop();
        }
        else
        {
            GameObject prefab = prefabs[effectName];
            GameObject go = Instantiate(prefab, transform);
            effect = go.GetComponent<EffectBase>();
            effect.OnPop();
        }

        return effect;
    }

    /// <summary>
    /// 풀에 반환
    /// </summary>
    public void ReturnEffect(string effectName, EffectBase effect)
    {
        if (!pools.ContainsKey(effectName))
        {
            pools[effectName] = new Queue<EffectBase>();
        }

        effect.OnPush();
        pools[effectName].Enqueue(effect);
    }
}