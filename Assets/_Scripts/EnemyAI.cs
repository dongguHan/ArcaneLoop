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
    public float attackRange = 1.5f;        // 공격 시작 범위
    public float attackWindupTime = 0.5f;   // 공격 준비 시간
    public float damageTime = 0.2f;         // 공격 판정 타이밍
    public float attackEndTime = 0.3f;      // 공격 후 마무리 시간
    public float attackCooldown = 1.5f;     // 공격 쿨다운

    private Rigidbody2D rb;
    private Transform targetPlayer;
    private SpriteRenderer spriteRenderer;

    private bool isDead = false;
    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        if (isDead) return;

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
        // 공격 시작: 이동 정지
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;

        // 공격 준비 (모션 예열)
        yield return new WaitForSeconds(attackWindupTime);

        // 공격 판정 (이 시점에서만 데미지)
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        if (distanceToPlayer <= attackRange)
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

        // 공격 후 모션 시간 (공격 후 자연스러운 딜레이)
        yield return new WaitForSeconds(attackEndTime);

        // 공격 모션 끝! → 바로 이동 가능
        isAttacking = false;

        // 다음 공격까지 쿨타임 (이동 가능, 공격만 금지)
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
}
