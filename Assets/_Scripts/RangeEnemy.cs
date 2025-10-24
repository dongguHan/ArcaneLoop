using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class RangeEnemy : MonoBehaviour
{
    [Header("Detection / Shooting")]
    public float detectionRange = 10f;
    public float fireRate = 1f;          // 초당 발사 횟수 (1 => 1초에 1발)
    public float bulletSpeed = 6f;
    public float bulletLifetime = 2f;
    public float shootAngleRandom = 7f;  // ± 각도 랜덤 오차

    [Header("References")]
    public BulletPool bulletPool;
    public LayerMask playerLayerMask;    // optional: 플레이어 레이어만 검사

    private float lastFireTime = 0f;
    private bool isDead = false;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    private Vector2 pushVelocity = Vector2.zero;
    private float pushTimeRemaining = 0f;
    private bool isBeingPushed = false;
    [Header("Push Settings")]
    public float pushDamping = 7.5f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if(isDead) return;

        if (isBeingPushed) return;

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

    private void FixedUpdate()
    {
        if(isBeingPushed && pushTimeRemaining > 0f)
        {
            Vector2 newPos = rb.position + pushVelocity * Time.fixedDeltaTime;
            rb.MovePosition(newPos);

            // 감속
            pushVelocity = Vector2.Lerp(pushVelocity, Vector2.zero, pushDamping * Time.fixedDeltaTime);

            pushTimeRemaining -= Time.fixedDeltaTime;
            if (pushTimeRemaining <= 0.01f || pushVelocity.sqrMagnitude < 0.001f)
            {
                isBeingPushed = false;
                pushVelocity = Vector2.zero;
                pushTimeRemaining = 0f;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.CompareTag("PlayerAttack"))
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        spriteRenderer.enabled = false;
        rb.simulated = false;
        StopAllCoroutines();
        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f);
        isDead = false;
        spriteRenderer.enabled = true;
        rb.simulated = true;
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
        Vector2 dir = (target.position - transform.position).normalized;

        // 각도 오차 추가
        float angleOffset = Random.Range(-shootAngleRandom, shootAngleRandom);
        Quaternion rotation = Quaternion.Euler(0, 0, angleOffset);
        dir = rotation * dir;

        GameObject bullet = BulletPool.Instance.GetBullet();
        bullet.transform.position = transform.position;

        EnemyBullet bulletComp = bullet.GetComponent<EnemyBullet>();
        bulletComp.Init(dir, bulletSpeed, bulletLifetime);
    }

    public void Push(Vector2 dir, float force, float duration)
    {
        if(isDead) return;
        if (dir == Vector2.zero) return;

        isBeingPushed = true;
        pushVelocity = dir.normalized * force;
        pushTimeRemaining = Mathf.Max(duration, 0.01f);
    }
}
