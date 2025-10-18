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
    public float maxMoveTime = 0.7f;

    [Header("Shared HP")]
    public int maxHealth = 5;
    private int currentHealth;

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
        mover.SetActive(true);

        Vector3 start = mover.transform.position;
        float distance = Vector3.Distance(start, targetPosition);
        float maxDistance = Vector3.Distance(Vector3.zero, new Vector3(1920f, 1080f, 0f));
        float moveTime = Mathf.Lerp(minMoveTime, maxMoveTime, distance / maxDistance);

        float elapsed = 0f;
        while (elapsed < moveTime)
        {
            mover.transform.position = Vector3.Lerp(start, targetPosition, elapsed / moveTime);
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
        currentHealth -= amount;
        Debug.Log($"Player HP: {currentHealth}");
        if (currentHealth <= 0)
            Die();
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
        {
            // Gray만 추적
            return playerGray.transform.position;
        }

        // 두 캐릭터 평균 위치 추적
        if (playerBlack.activeSelf && playerWhite.activeSelf)
        {
            return (playerBlack.transform.position + playerWhite.transform.position) * 0.5f;
        }

        // 예외적으로 하나만 활성일 경우 대비
        if (playerBlack.activeSelf)
            return playerBlack.transform.position;
        if (playerWhite.activeSelf)
            return playerWhite.transform.position;

        // 기본값
        return transform.position;
    }
}
