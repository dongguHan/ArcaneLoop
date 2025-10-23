using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Objects")]
    public GameObject playerBlack;
    public GameObject playerWhite;
    public GameObject playerGray;

    [Header("Movement Time Settings")]
    public float minMoveTime = 0.1f;
    public float maxMoveTime = 1f;

    [Header("Enemy Push Settings")]
    public float pushForce = 10f;               // 적을 밀어내는 힘
    public float pushRadius = 0.6f;            // 적 감지 반경
    public LayerMask enemyLayer;               // Enemy 레이어

    [Header("Shared HP")]
    public int maxHealth = 5;
    private int currentHealth;
    private bool isTransforming = false;

    [HideInInspector] public bool isBlack = false;
    [HideInInspector] public bool isTransform = false;

    public bool IsGrayActive => playerGray != null && playerGray.activeSelf;

    void Start()
    {
        currentHealth = maxHealth;
        playerBlack.SetActive(true);
        playerWhite.SetActive(true);
        playerGray.SetActive(false);
    }

    void Update()
    {
        HandleTransformInput();
    }

    private void HandleTransformInput()
    {
        if (!playerGray.activeSelf && !isTransform)
        {
            if (Keyboard.current.shiftKey.wasPressedThisFrame)
                StartCoroutine(MoveAndActivateGray(playerWhite, playerBlack.transform.position, true));
            else if (Keyboard.current.spaceKey.wasPressedThisFrame)
                StartCoroutine(MoveAndActivateGray(playerBlack, playerWhite.transform.position, false));
        }
        else if (playerGray.activeSelf && isTransform && !playerGray.GetComponent<PlayerAttack>().isAttacking)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
                DeactivateGray();
        }
    }

    private IEnumerator MoveAndActivateGray(GameObject mover, Vector3 targetPosition, bool blackActive)
    {
        isTransforming = true;
        mover.SetActive(true);

        Vector3 start = mover.transform.position;
        Vector3 moveDir = (targetPosition - start).normalized;

        float distance = Vector3.Distance(start, targetPosition);
        float maxDistance = Vector3.Distance(Vector3.zero, new Vector3(1920f, 1080f, 0f));
        float moveTime = Mathf.Lerp(minMoveTime, maxMoveTime, distance / maxDistance);

        float elapsed = 0f;
        while (elapsed < moveTime)
        {
            Vector3 currentPos = Vector3.Lerp(start, targetPosition, elapsed / moveTime);

            // 이동 경로상의 적 밀기
            PushEnemies(currentPos, moveDir);

            mover.transform.position = currentPos;
            elapsed += Time.deltaTime;
            yield return null;
        }

        mover.transform.position = targetPosition;

        // Gray 활성화
        playerGray.transform.position = targetPosition;
        playerGray.SetActive(true);

        // Black/White 비활성화
        playerBlack.SetActive(false);
        playerWhite.SetActive(false);

        mover.SetActive(false);
        isBlack = blackActive;
        isTransform = true;
        isTransforming = false;
    }

    private void PushEnemies(Vector3 playerPos, Vector2 moveDir)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(playerPos, pushRadius, enemyLayer);
        foreach (Collider2D col in hitEnemies)
        {
            // 플레이어가 바라보는 방향 기준 "수직 방향" 계산
            Vector2 pushDir = Vector2.Perpendicular(moveDir).normalized;

            // 좌우 랜덤 선택 (왼쪽 또는 오른쪽)
            if (Random.value > 0.5f)
                pushDir = -pushDir;

            switch (col.gameObject.name)
            {
                case "BasicEnemy":
                    EnemyAI enemy = col.GetComponent<EnemyAI>();
                    enemy.Push(pushDir, pushForce, 0.2f);
                    break;
                case "BulletEnemy":
                    RangeEnemy rangeEnemy = col.GetComponent<RangeEnemy>();
                    rangeEnemy.Push(pushDir, pushForce, 0.2f);
                    break;
                default:
                    Debug.LogError("WrongName");
                    break;
            }
        }
    }

    private void DeactivateGray()
    {
        Vector3 pos = playerGray.transform.position;
        playerGray.SetActive(false);

        playerBlack.transform.position = pos;
        playerWhite.transform.position = pos;

        playerBlack.SetActive(true);
        playerWhite.SetActive(true);

        isTransform = false;
    }

    public void TakeDamage(int amount)
    {
        if (!isTransforming)
        {
            currentHealth -= amount;
            Debug.Log($"Player HP: {currentHealth}");
            if (currentHealth <= 0)
                Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player Dead");
        // TODO: 리스폰 혹은 게임 오버 처리
    }

    // 카메라용 중심 좌표 계산
    public Vector3 GetCameraTargetPosition()
    {
        if (IsGrayActive)
            return playerGray.transform.position;

        if (playerBlack.activeSelf && playerWhite.activeSelf)
            return (playerBlack.transform.position + playerWhite.transform.position) * 0.5f;

        if (playerBlack.activeSelf)
            return playerBlack.transform.position;
        if (playerWhite.activeSelf)
            return playerWhite.transform.position;

        return transform.position;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (playerBlack != null)
            Gizmos.DrawWireSphere(playerBlack.transform.position, pushRadius);
        if (playerWhite != null)
            Gizmos.DrawWireSphere(playerWhite.transform.position, pushRadius);
    }
}
