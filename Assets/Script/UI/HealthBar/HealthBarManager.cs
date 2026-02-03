using System.Collections.Generic;
using UnityEngine;

public class HealthBarManager : MonoBehaviour
{
    public static HealthBarManager Instance;

    [Header("Pool Parent")]
    [SerializeField] private Transform healthBarPool;  // HealthBarPool 드래그

    [Header("Prefab")]
    [SerializeField] private GameObject healthBarPrefab;

    private Queue<HealthBarUI> pool = new Queue<HealthBarUI>();
    private List<HealthBarUI> activeHealthBars = new List<HealthBarUI>();

    [SerializeField] private int initialPoolSize = 50;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // ✅ healthBarPool이 null이면 자동으로 자식에서 찾기
        if (healthBarPool == null)
        {
            healthBarPool = transform.Find("HealthBarPool");

            // 없으면 생성
            if (healthBarPool == null)
            {
                GameObject poolObj = new GameObject("HealthBarPool");
                poolObj.transform.SetParent(transform);
                healthBarPool = poolObj.AddComponent<RectTransform>();
            }
        }

        // 초기 풀 생성
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateHealthBar();
        }
    }

    HealthBarUI CreateHealthBar()
    {
        GameObject obj = Instantiate(healthBarPrefab, healthBarPool);
        HealthBarUI healthBar = obj.GetComponent<HealthBarUI>();
        obj.SetActive(false);
        pool.Enqueue(healthBar);
        return healthBar;
    }

    public HealthBarUI GetHealthBar(Transform target)
    {
        HealthBarUI healthBar;

        if (pool.Count > 0)
        {
            healthBar = pool.Dequeue();
        }
        else
        {
            healthBar = CreateHealthBar();
        }

        healthBar.gameObject.SetActive(true);
        healthBar.Initialize(target);
        activeHealthBars.Add(healthBar);

        return healthBar;
    }

    public void ReturnHealthBar(HealthBarUI healthBar)
    {
        if (healthBar == null) return;

        activeHealthBars.Remove(healthBar);
        healthBar.gameObject.SetActive(false);
        pool.Enqueue(healthBar);
    }

    public void ClearAll()
    {
        foreach (var healthBar in activeHealthBars)
        {
            healthBar.gameObject.SetActive(false);
            pool.Enqueue(healthBar);
        }
        activeHealthBars.Clear();
    }
}