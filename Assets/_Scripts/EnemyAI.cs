using UnityEngine;
using System.Collections;

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

    private bool isDead = false;
    private bool isAttacking = false;

    // === Push 관련 ===
    private Vector2 pushVelocity = Vector2.zero;
    private float pushTimeRemaining = 0f;
    private bool isBeingPushed = false;
    [Header("Push Settings")]
    public float pushDamping = 5f;  // 감속 속도 (클수록 빨리 멈춤)

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
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
        // Push 중일 때만 처리
        if (isBeingPushed && pushTimeRemaining > 0f)
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

    void FollowPlayer()
    {
        Vector2 direction = (targetPlayer.position - transform.position).normalized;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, obstacleCheckDistance, obstacleLayer);
        if (hit.collider != null)
        {
            Vector2 perpDirection = Vector2.Perpendicular(direction);
            direction = (Random.value > 0.5f) ? perpDirection : -perpDirection;
        }

        Vector2 newPos = (Vector2)transform.position + direction * speed * Time.deltaTime;
        rb.MovePosition(newPos);

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
