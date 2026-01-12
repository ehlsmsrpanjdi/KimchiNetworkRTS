using System.Collections.Generic;
using UnityEngine;

public interface IBulletMovement
{
    /// <summary>
    /// 총알 이동 로직
    /// </summary>
    void Move(BulletBase bullet, float deltaTime);

    /// <summary>
    /// 타겟에 도달했는지 체크
    /// </summary>
    bool HasReachedTarget(BulletBase bullet);
}


public class BulletMovementManager
{
    static BulletMovementManager instance;
    public static BulletMovementManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new BulletMovementManager();
                instance.Initialize();
            }
            return instance;
        }
    }

    private Dictionary<int, IBulletMovement> movements = new Dictionary<int, IBulletMovement>();

    void Initialize()
    {
        movements[1] = new LinearMovement();
        movements[2] = new ParabolicMovement();
        movements[3] = new HomingMovement();

        LogHelper.Log("✅ BulletMovementManager initialized");
    }

    public IBulletMovement GetMovement(int movementID)
    {
        if (movements.TryGetValue(movementID, out IBulletMovement movement))
        {
            return movement;
        }

        LogHelper.LogError($"BulletMovement not found: {movementID}, using Linear as default");
        return movements[1]; // 기본값: Linear
    }

    public bool HasMovement(int movementID)
    {
        return movements.ContainsKey(movementID);
    }
}






// 직선 이동
/// <summary>
/// 직선 이동 (발사 시점의 타겟 위치로 이동)
/// </summary>
public class LinearMovement : IBulletMovement
{
    public void Move(BulletBase bullet, float deltaTime)
    {
        Vector3 direction = (bullet.targetPosition - bullet.transform.position).normalized;
        bullet.transform.position += direction * bullet.moveSpeed * deltaTime;
    }

    public bool HasReachedTarget(BulletBase bullet)
    {
        return Vector3.Distance(bullet.transform.position, bullet.targetPosition) < 0.1f;
    }
}

/// <summary>
/// 포물선 이동 (발사 시점의 타겟 위치로 포물선을 그리며 이동)
/// </summary>
public class ParabolicMovement : IBulletMovement
{
    private float arcHeight = 2f; // 포물선 높이

    public void Move(BulletBase bullet, float deltaTime)
    {
        if (bullet.journeyStartTime < 0f)
        {
            // 첫 이동 시 초기화
            bullet.journeyStartTime = Time.time;
            bullet.journeyStartPosition = bullet.transform.position;
            bullet.journeyDistance = Vector3.Distance(bullet.journeyStartPosition, bullet.targetPosition);
            bullet.journeyDuration = bullet.journeyDistance / bullet.moveSpeed;
        }

        float elapsedTime = Time.time - bullet.journeyStartTime;
        float progress = elapsedTime / bullet.journeyDuration;

        if (progress > 1f)
        {
            bullet.transform.position = bullet.targetPosition;
            return;
        }

        // 수평 이동 (직선)
        Vector3 horizontalPosition = Vector3.Lerp(bullet.journeyStartPosition, bullet.targetPosition, progress);

        // 수직 이동 (포물선)
        float verticalOffset = arcHeight * Mathf.Sin(progress * Mathf.PI);

        bullet.transform.position = horizontalPosition + Vector3.up * verticalOffset;
    }

    public bool HasReachedTarget(BulletBase bullet)
    {
        if (bullet.journeyStartTime < 0f) return false;

        float elapsedTime = Time.time - bullet.journeyStartTime;
        return elapsedTime >= bullet.journeyDuration;
    }
}

// 호밍 (타겟 추적)
/// <summary>
/// 호밍 이동 (타겟 몬스터를 추적)
/// </summary>
public class HomingMovement : IBulletMovement
{
    public void Move(BulletBase bullet, float deltaTime)
    {
        // 타겟이 죽었으면 마지막 위치로 이동
        Vector3 targetPos = (bullet.targetMonster != null && bullet.targetMonster.gameObject.activeSelf)
            ? bullet.targetMonster.transform.position
            : bullet.targetPosition;

        Vector3 direction = (targetPos - bullet.transform.position).normalized;
        bullet.transform.position += direction * bullet.moveSpeed * deltaTime;

        // 타겟 방향으로 회전
        if (direction != Vector3.zero)
        {
            bullet.transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    public bool HasReachedTarget(BulletBase bullet)
    {
        // 타겟이 살아있으면 타겟 위치 기준
        if (bullet.targetMonster != null && bullet.targetMonster.gameObject.activeSelf)
        {
            return Vector3.Distance(bullet.transform.position, bullet.targetMonster.transform.position) < 0.2f;
        }

        // 타겟이 죽었으면 마지막 위치 기준
        return Vector3.Distance(bullet.transform.position, bullet.targetPosition) < 0.2f;
    }
}