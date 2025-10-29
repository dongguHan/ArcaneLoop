using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    [Header("Move")]
    public float speed = 5f;

    [Header("Refs")]
    public PlayerManager playerManager;

    // --- internals ---
    private Rigidbody2D rb;
    private Vector2 movement;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    // 마지막 바라보는 방향(오른쪽=+1, 왼쪽=-1)
    private float lastFacingX = 1f;

    [Header("Blocked by Tilemap")]
    public LayerMask wallLayer;
    public LayerMask waterLayer;
    public LayerMask objectLayer;
    private CapsuleCollider2D capsule;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsule = GetComponent<CapsuleCollider2D>();

        // 자식에 붙어 있어도 안전하게 찾기
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        HandleInput();

        // 애니메이션 파라미터 세팅 (걷는 중 여부만)
        bool isWalking = movement.sqrMagnitude > 0.00001f;
        if (animator != null)
            animator.SetBool("IsWalking", isWalking);

        // 바라보는 방향 유지(Idle에서도 유지)
        if (spriteRenderer != null)
        {
            if (Mathf.Abs(movement.x) > 0.0001f)
                lastFacingX = Mathf.Sign(movement.x);

            // 스프라이트 기본이 "오른쪽"을 본다고 가정
            spriteRenderer.flipX = (lastFacingX < 0f);
        }
    }

    void FixedUpdate()
    {
        if (movement == Vector2.zero) return;

        Vector2 move = movement * speed * Time.fixedDeltaTime;

        // 현재는 임의로 타일 레이어 이름 지정 만약 바꾸면 수정 필요
        LayerMask combinedMask;
        if (gameObject.CompareTag("PlayerBlack") || playerManager.isBlack)
            combinedMask = LayerMask.GetMask("WallTile", "WaterTile", "ObjectTile");

        else
            combinedMask = LayerMask.GetMask("WallTile");

        // 이동하려는 방향으로 CapsuleCast 실행
        RaycastHit2D hit = Physics2D.CapsuleCast(
            rb.position,
            capsule.size,
            capsule.direction,
            0f,
            movement,
            move.magnitude,
            combinedMask
        );

        // 충돌 없을 때만 이동
        if (!hit)
        {
            rb.MovePosition(rb.position + move);
        }
        else
        {
            // 충돌이 있을 경우, 벽에 닿기 직전까지만 이동
            float distance = Mathf.Max(hit.distance - 0.01f, 0f);
            rb.MovePosition(rb.position + movement * distance);
        }
    }

    private void HandleInput()
    {
        Vector2 moveInput = Vector2.zero;

        if (gameObject.CompareTag("PlayerBlack"))
        {
            if (Keyboard.current.wKey.isPressed) moveInput.y += 1;
            if (Keyboard.current.sKey.isPressed) moveInput.y -= 1;
            if (Keyboard.current.aKey.isPressed) moveInput.x -= 1;
            if (Keyboard.current.dKey.isPressed) moveInput.x += 1;
        }
        else if (gameObject.CompareTag("PlayerWhite"))
        {
            if (Keyboard.current.upArrowKey.isPressed) moveInput.y += 1;
            if (Keyboard.current.downArrowKey.isPressed) moveInput.y -= 1;
            if (Keyboard.current.leftArrowKey.isPressed) moveInput.x -= 1;
            if (Keyboard.current.rightArrowKey.isPressed) moveInput.x += 1;
        }
        else if (gameObject.CompareTag("PlayerGray") && playerManager != null && playerManager.isTransform)
        {
            if (playerManager.isBlack)
            {
                if (Keyboard.current.wKey.isPressed) moveInput.y += 1;
                if (Keyboard.current.sKey.isPressed) moveInput.y -= 1;
                if (Keyboard.current.aKey.isPressed) moveInput.x -= 1;
                if (Keyboard.current.dKey.isPressed) moveInput.x += 1;
            }
            else
            {
                if (Keyboard.current.upArrowKey.isPressed) moveInput.y += 1;
                if (Keyboard.current.downArrowKey.isPressed) moveInput.y -= 1;
                if (Keyboard.current.leftArrowKey.isPressed) moveInput.x -= 1;
                if (Keyboard.current.rightArrowKey.isPressed) moveInput.x += 1;
            }
        }

        if (moveInput != Vector2.zero)
            moveInput.Normalize();

        movement = moveInput;
    }
}
