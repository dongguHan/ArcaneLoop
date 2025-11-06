using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

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
    private Transform target;

    private float lastFireTime = 0f;
    private bool isDead = false;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private BoxCollider2D box;

    private Vector2 pushVelocity = Vector2.zero;
    private float pushTimeRemaining = 0f;
    private bool isBeingPushed = false;
    private bool isBeingAttackPushed = false;
    [Header("Push Settings")]
    public float pushDamping = 7.5f;

    [Header("Health Point")]
    private HashSet<int> recentAttackIds = new HashSet<int>();
    private Queue<int> attackIdQueue = new Queue<int>();
    private const int MaxStoredIds = 10;
    private float health;
    public float maxHealth = 2;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        box = GetComponent<BoxCollider2D>();
        health = maxHealth;
    }

    void Update()
    {
        if(isDead) return;

        if (isBeingPushed) return;

        // 사거리 내 가장 가까운 플레이어 찾기
        target = FindNearestPlayerInRange(detectionRange);
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

    void FixedUpdate()
    {
        if ((isBeingPushed || isBeingAttackPushed) && pushTimeRemaining > 0f)
        {
            Vector2 dir = pushVelocity.normalized;
            float moveDist = pushVelocity.magnitude * Time.fixedDeltaTime;

            Vector2 boxCenter = rb.position + box.offset;
            Vector2 halfSize = box.size * 0.5f; // 절반 크기 계산

            LayerMask combinedMask = LayerMask.GetMask("WallTile", "WaterTile", "ObjectTile");

            // 콜라이더의 반쪽 길이만큼 더 검사
            RaycastHit2D hit = Physics2D.BoxCast(
                boxCenter,
                box.size,
                0f,
                dir,
                moveDist + Mathf.Max(halfSize.x, halfSize.y), // 가장 긴 변 기준
                combinedMask
            );

            if (hit.collider != null)
            {
                if (isBeingAttackPushed)
                {
                    // 공격 넉백은 벽에서 튕김
                    Vector2 reflect = Vector2.Reflect(pushVelocity, hit.normal);
                    pushVelocity = reflect;
                    return;
                }
                else
                {
                    // 기존 push는 stop
                    rb.MovePosition(rb.position + dir * hit.distance);
                    StopPush();
                    return;
                }
            }

            // 이동
            rb.MovePosition(rb.position + dir * moveDist);

            // 감속
            pushVelocity = Vector2.Lerp(pushVelocity, Vector2.zero, pushDamping * Time.fixedDeltaTime);
            pushTimeRemaining -= Time.fixedDeltaTime;

            if (pushTimeRemaining <= 0.01f || pushVelocity.sqrMagnitude < 0.001f)
            {
                StopPush();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("PlayerAttack"))
        {
            AttackObject attack = other.GetComponent<AttackObject>();
            if (attack == null) return;

            int id = attack.attackId;

            if (recentAttackIds.Contains(id))
                return;

            recentAttackIds.Add(id);
            attackIdQueue.Enqueue(id);

            if (attackIdQueue.Count > MaxStoredIds)
            {
                int oldId = attackIdQueue.Dequeue();
                recentAttackIds.Remove(oldId);
            }

            GetDamage(attack.isBlackAttack);
        }
    }

    void GetDamage(bool attackType)
    {
        --health;
        CameraShakeManager.Instance.Shake(0.1f, 0.1f);

        if (health <= 0)
        {
            Die();
            return;
        }

        if (attackType)
        {
            // 공격으로 인해 넉백
            Vector2 attackDir = (transform.position - target.position).normalized;
            StartAttackPush(attackDir, 8f, 0.25f); // force랑 duration은 너가 tune
        }
    }

    void StartAttackPush(Vector2 dir, float force, float duration)
    {
        isBeingAttackPushed = true;
        isBeingPushed = false; // 기존 push는 off
        pushVelocity = dir.normalized * force;
        pushTimeRemaining = Mathf.Max(duration, 0.01f);
    }

    void Die()
    {
        isDead = true;
        StopPush();

        spriteRenderer.enabled = false;
        rb.linearVelocity = Vector2.zero;  // 속도 정지
        rb.simulated = false;

        StopAllCoroutines();
        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f);

        // 내부 상태 초기화
        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        isDead = false;
        StopPush();

        health = maxHealth;

        spriteRenderer.enabled = true;
    }

    void StopPush()
    {
        isBeingPushed = false;
        isBeingAttackPushed = false;
        pushVelocity = Vector2.zero;
        pushTimeRemaining = 0f;
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
