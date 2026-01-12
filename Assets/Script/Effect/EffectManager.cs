using System.Collections.Generic;
using UnityEngine;

public class EffectManager
{
    private static EffectManager instance;
    public static EffectManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new EffectManager();
                instance.Initialize();
            }
            return instance;
        }
    }

    private Dictionary<string, Queue<EffectBase>> pools = new Dictionary<string, Queue<EffectBase>>();
    private Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

    void Initialize()
    {
        LoadEffects();
    }

    void LoadEffects()
    {
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
            GameObject go = Object.Instantiate(prefab);  // ✅ transform 부모 없이
            effect = go.GetComponent<EffectBase>();
            effect.OnPop();
        }

        return effect;
    }

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