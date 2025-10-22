using System.Collections.Generic;
using UnityEngine;

public class RangeEnemy : MonoBehaviour
{
    [Header("Detection / Shooting")]
    public float detectionRange = 10f;
    public float fireRate = 1f;          // 초당 발사 횟수 (1 => 1초에 1발)
    public float bulletSpeed = 6f;
    public float bulletLifetime = 2f;
    public float shootAngleRandom = 0f;  // ± 각도 랜덤 오차

    [Header("References")]
    public BulletPool bulletPool;
    public LayerMask playerLayerMask;    // optional: 플레이어 레이어만 검사

    float lastFireTime = 0f;

    void Update()
    {
        // 사거리 내 가장 가까운 플레이어 찾기
        Transform target = FindNearestPlayerInRange(detectionRange);
        if (target == null)
        {
            return;
        }

        // 쿨다운 체크
        if (Time.time - lastFireTime >= 1f / Mathf.Max(0.0001f, fireRate))
        {
            ShootAt(target);
            lastFireTime = Time.time;
        }
    }

    Transform FindNearestPlayerInRange(float range)
    {
        // 태그 기반 검색(Gray, Black, White 우선순위는 필요시 적용)
        GameObject[] candidates = GameObject.FindGameObjectsWithTag("PlayerGray");
        if (candidates.Length == 0 || !HasValid(candidates))
        {
            GameObject[] blacks = GameObject.FindGameObjectsWithTag("PlayerBlack");
            GameObject[] whites = GameObject.FindGameObjectsWithTag("PlayerWhite");
            // combine
            List<GameObject> list = new List<GameObject>();
            list.AddRange(blacks);
            list.AddRange(whites);
            candidates = list.ToArray();
        }

        float minDist = Mathf.Infinity;
        Transform nearest = null;
        Vector3 pos = transform.position;

        foreach (var go in candidates)
        {
            if (go == null || !go.activeInHierarchy) continue;
            float d = Vector2.Distance(pos, go.transform.position);
            if (d <= range && d < minDist)
            {
                minDist = d;
                nearest = go.transform;
            }
        }
        return nearest;
    }

    bool HasValid(GameObject[] arr)
    {
        foreach (var g in arr) if (g != null && g.activeInHierarchy) return true;
        return false;
    }

    void ShootAt(Transform target)
    {
        if (BulletPool.Instance == null)
        {
            Debug.LogWarning("No BulletPool instance found.");
            return;
        }

        Vector2 dir = (target.position - transform.position).normalized;
        GameObject b = BulletPool.Instance.GetBullet();
        b.transform.position = transform.position;
        EnemyBullet bulletComp = b.GetComponent<EnemyBullet>();
        if (bulletComp != null)
        {
            bulletComp.Init(dir, bulletSpeed, bulletLifetime);
        }
        else
        {
            Rigidbody2D rb = b.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = dir * bulletSpeed;
            b.SetActive(true);
        }
    }
}
