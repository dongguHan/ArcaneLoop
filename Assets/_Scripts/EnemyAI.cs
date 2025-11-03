using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 2f;
    public float detectionRange = 10f;
    public float obstacleCheckDistance = 1f;
    public LayerMask obstacleLayer;

    [Header("Attack Settings")]
    public float attackRange = 1.5f;
    public float attackWindupTime = 0.5f;
    public float damageTime = 0.2f;
    public float attackEndTime = 0.3f;
    public float attackCooldown = 1.5f;

    private Rigidbody2D rb;
    private Transform targetPlayer;
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D box;  // sprite 추가시 Collider도 바꾸기

    private bool isDead = false;
    private bool isAttacking = false;

    // === Push 관련 ===
    private Vector2 pushVelocity = Vector2.zero;
    private float pushTimeRemaining = 0f;
    private bool isBeingPushed = false;
    private bool isBeingAttackPushed = false; // 공격 넉백용 push
    [Header("Push Settings")]
    public float pushDamping = 5f;  // 감속 속도 (클수록 빨리 멈춤)

    [Header("Health Point")]
    private HashSet<int> recentAttackIds = new HashSet<int>();
    private Queue<int> attackIdQueue = new Queue<int>();
    private const int MaxStoredIds = 10;
    private float health;
    public float maxHealth = 3;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        box= GetComponent<BoxCollider2D>();
        health = maxHealth;
    }

    void Update()
    {
        if (isDead) return;

        // 밀리는 중이면 AI 이동 중단
        if (isBeingPushed)
            return;

        FindNearestPlayer();

        if (targetPlayer != null && !isAttacking)
        {
            float dist = Vector2.Distance(transform.position, targetPlayer.position);

            if (dist <= attackRange)
            {
                StartCoroutine(AttackRoutine(targetPlayer.gameObject));
            }
            else
            {
                FollowPlayer();
            }
        }
    }

    void FixedUpdate()
    {
        if ((isBeingPushed || isBeingAttackPushed) && pushTimeRemaining > 0f)
        {
            Vector2 dir = pushVelocity.normalized;
            float moveDist = pushVelocity.magnitude * Time.fixedDeltaTime;

            Vector2 boxCenter = rb.position + box.offset;
            Vector2 halfSize = box.size * 0.5f;
            LayerMask combinedMask = LayerMask.GetMask("WallTile", "WaterTile", "ObjectTile");

            RaycastHit2D hit = Physics2D.BoxCast(
                boxCenter,
                box.size,
                0f,
                dir,
                moveDist + Mathf.Max(halfSize.x, halfSize.y),
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

            rb.MovePosition(rb.position + dir * moveDist);

            pushVelocity = Vector2.Lerp(pushVelocity, Vector2.zero, pushDamping * Time.fixedDeltaTime);
            pushTimeRemaining -= Time.fixedDeltaTime;

            if (pushTimeRemaining <= 0.01f || pushVelocity.sqrMagnitude < 0.001f)
            {
                StopPush();
            }
        }
    }

    void FollowPlayer()
    {
        if (targetPlayer == null) return;

        Vector2 direction = (targetPlayer.position - transform.position).normalized;

        // BoxCollider2D 정보 가져오기
        Vector2 boxSize = box != null ? box.size : Vector2.one * 0.5f;
        float moveDist = speed * Time.deltaTime;
        LayerMask combinedMask = LayerMask.GetMask("WallTile", "WaterTile", "ObjectTile");

        // 이동 경로에 충돌 검사
        RaycastHit2D hit = Physics2D.BoxCast(rb.position, boxSize, 0f, direction, moveDist, combinedMask);

        if (hit.collider != null)
        {
            // 벽 표면 방향 (법선에 수직)
            Vector2 wallTangent = new Vector2(-hit.normal.y, hit.normal.x);

            // 플레이어의 위치를 기준으로 벽 위/오른쪽 방향 중 가까운 쪽으로 슬라이드
            float dot = Vector2.Dot(direction, wallTangent);
            if (dot < 0)
                wallTangent = -wallTangent;

            // 새 이동 방향 = 벽을 따라가는 방향
            direction = wallTangent.normalized;
        }

        // 이동
        Vector2 newPos = rb.position + direction * moveDist;
        rb.MovePosition(newPos);

        // 시각 방향 (왼쪽/오른쪽)
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = (targetPlayer.position.x < transform.position.x);
        }
    }


    IEnumerator AttackRoutine(GameObject player)
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(attackWindupTime);

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        if (distanceToPlayer <= attackRange && player.activeSelf)
        {
            PlayerInvulnerability invuln = player.GetComponent<PlayerInvulnerability>();
            if (invuln != null && invuln.CanTakeDamage())
            {
                PlayerManager pm = player.GetComponentInParent<PlayerManager>();
                if (pm != null)
                {
                    pm.TakeDamage(1);
                    invuln.StartInvuln();
                }
            }
        }

        yield return new WaitForSeconds(attackEndTime);
        isAttacking = false;

        yield return new WaitForSeconds(attackCooldown);
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

        if (health <= 0)
        {
            Die();
            return;
        }

        if (attackType)
        {
            // 공격으로 인해 넉백
            Vector2 attackDir = (transform.position - targetPlayer.position).normalized;
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
        isAttacking = false;   // 공격 상태 초기화
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
        isAttacking = false;
        StopPush();

        health = maxHealth;

        spriteRenderer.enabled = true;

        // 리스폰 후 바로 타겟 재탐색
        FindNearestPlayer();
    }

    void StopPush()
    {
        isBeingPushed = false;
        isBeingAttackPushed = false;
        pushVelocity = Vector2.zero;
        pushTimeRemaining = 0f;
    }

    void FindNearestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("PlayerGray");

        if (players.Length == 0 || !HasValidPlayer(players))
        {
            players = GameObject.FindGameObjectsWithTag("PlayerBlack");
            GameObject[] whites = GameObject.FindGameObjectsWithTag("PlayerWhite");
            players = CombineArrays(players, whites);
        }

        float minDist = Mathf.Infinity;
        Transform nearest = null;

        foreach (GameObject player in players)
        {
            if (!IsValidPlayer(player)) continue;

            float dist = Vector2.Distance(transform.position, player.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = player.transform;
            }
        }

        targetPlayer = nearest;
    }

    GameObject[] CombineArrays(GameObject[] a1, GameObject[] a2)
    {
        GameObject[] result = new GameObject[a1.Length + a2.Length];
        a1.CopyTo(result, 0);
        a2.CopyTo(result, a1.Length);
        return result;
    }

    bool IsValidPlayer(GameObject player)
    {
        return player != null && player.activeInHierarchy;
    }

    bool HasValidPlayer(GameObject[] players)
    {
        foreach (GameObject p in players)
        {
            if (IsValidPlayer(p)) return true;
        }
        return false;
    }

    // === 여기서부터 Push 관련 추가 ===
    public void Push(Vector2 dir, float force, float duration)
    {
        if (isDead) return;
        if (dir == Vector2.zero) return;

        // 이동 중이던 AI를 잠시 멈춤
        isBeingPushed = true;
        pushVelocity = dir.normalized * force;
        pushTimeRemaining = Mathf.Max(duration, 0.01f);

        // 공격 중이라면 공격 취소 가능하게 하려면 아래 주석 해제
        isAttacking = false;

        // 디버그 확인용
        // Debug.Log($"{name} pushed dir={dir}, force={force}, duration={duration}");
    }
}
