using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyBullet : MonoBehaviour
{
    Rigidbody2D rb;
    Vector2 dir;
    float speed;
    float lifetime;
    float spawnTime;

    [Header("Blocked by Tilemap")]
    public LayerMask wallLayer;
    public LayerMask objectLayer;
    private CapsuleCollider2D capsule;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsule = GetComponent<CapsuleCollider2D>();
    }

    public void Init(Vector2 direction, float spd, float life)
    {
        dir = direction.normalized;
        speed = spd;
        lifetime = life;
        spawnTime = Time.time;

        // 방향에 맞게 회전 (원하면 사용)
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // 초기 속도 설정 (Rigidbody2D는 kinematic이면 velocity 사용 가능)
        if (rb != null)
        {
            rb.linearVelocity = dir * speed;
        }

        gameObject.SetActive(true);
    }

    void Update()
    {
        if (Time.time - spawnTime >= lifetime)
        {
            ReturnToPool();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 플레이어에 맞으면 데미지 주고 반환
        if (other.CompareTag("PlayerBlack") || other.CompareTag("PlayerWhite") || other.CompareTag("PlayerGray"))
        {
            // 통합된 데미지 API가 PlayerManager라면
            PlayerInvulnerability invuln = other.GetComponent<PlayerInvulnerability>();
            if (invuln != null && invuln.CanTakeDamage() && other.gameObject.activeSelf)
            {
                PlayerManager pm = other.GetComponentInParent<PlayerManager>();
                if (pm != null)
                {
                    pm.TakeDamage(1);
                    invuln.StartInvuln();
                }
                ReturnToPool();
                return;
            }
        }

        // 2) 벽/오브젝트 레이어 검사 (LayerMask로 안전하게 처리)
        if (IsInLayerMask(other.gameObject, wallLayer) || IsInLayerMask(other.gameObject, objectLayer))
        {
            ReturnToPool();
            return;
        }
    }

    // 헬퍼: GameObject가 LayerMask에 포함되는지 체크
    private bool IsInLayerMask(GameObject obj, LayerMask mask)
    {
        return ((mask.value & (1 << obj.layer)) != 0);
    }

    void ReturnToPool()
    {
        // 속도 리셋
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // 풀이 존재하면 반환, 없으면 비활성화
        if (BulletPool.Instance != null)
            BulletPool.Instance.ReturnBullet(gameObject);
        else
            gameObject.SetActive(false);
    }

    // 안전: 비활성화 시 속도 초기화
    void OnDisable()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }
}
