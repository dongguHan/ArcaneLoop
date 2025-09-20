using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    public float speed = 5f;
    public PlayerManager playerManager;
    private Rigidbody2D rb;
    private Vector2 movement;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
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
        else if (gameObject.CompareTag("PlayerGray") && playerManager.isTransform)
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
        {
            moveInput.Normalize();
        }
        movement = moveInput;

        // ÁÂ¿ì ¹ÝÀü
        if (spriteRenderer != null && moveInput.x != 0)
        {
            spriteRenderer.flipX = (moveInput.x > 0);
        }
    }
}
