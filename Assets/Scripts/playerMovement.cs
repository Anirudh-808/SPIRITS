using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // No gravity for top-down
        rb.gravityScale = 0f;

        gameObject.layer = 0;
        spriteRenderer.sortingOrder = 1;
    }

    void Update()
    {
        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized; // normalized prevents faster diagonal movement

        // Flip sprite horizontally based on left/right
        if (moveInput.x != 0)
            spriteRenderer.flipX = moveInput.x < 0;
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }
}