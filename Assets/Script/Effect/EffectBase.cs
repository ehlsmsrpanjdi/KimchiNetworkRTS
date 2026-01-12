using UnityEngine;

/// <summary>
/// 이펙트 기본 클래스 (파티클, 애니메이션 등)
/// </summary>
public class EffectBase : MonoBehaviour, IPoolObj
{
    [Header("Effect Settings")]
    public string effectName;
    public float duration = 2f;  // 자동 반환 시간

    private float spawnTime;
    private bool isPlaying;

    void Update()
    {
        if (!isPlaying) return;

        // 지속시간 지나면 자동 반환
        if (Time.time - spawnTime >= duration)
        {
            ReturnToPool();
        }
    }

    public void Play(Vector3 position, Quaternion rotation)
    {
        transform.SetPositionAndRotation(position, rotation);
        spawnTime = Time.time;
        isPlaying = true;

        // 파티클 시스템 재생
        var particles = GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particles)
        {
            ps.Play();
        }

        // 애니메이터 재생
        var animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.Play(0);
        }
    }

    void ReturnToPool()
    {
        isPlaying = false;
        EffectManager.Instance.ReturnEffect(effectName, this);
    }

    // ========== IPoolObj 구현 ==========
    public void OnPop()
    {
        gameObject.SetActive(true);
    }

    public void OnPush()
    {
        gameObject.SetActive(false);
        isPlaying = false;

        // 파티클 정지
        var particles = GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particles)
        {
            ps.Stop();
            ps.Clear();
        }
    }
}