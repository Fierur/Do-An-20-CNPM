using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class TestMovementPatternAndJump : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;
    public float coyoteTime = 0.1f;          // Cho phép nhảy trong vài ms sau khi rời mặt đất
    public float jumpBufferTime = 0.1f;      // Giữ lệnh nhảy trong vài ms trước khi chạm đất

    [Header("Ground Check Settings")]
    public Transform groundCheck;
    public Vector2 groundBoxSize = new Vector2(0.5f, 0.1f);
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private bool isGrounded;
    private float lastGroundedTime;
    private float lastJumpPressedTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true; // tránh nhân vật xoay lung tung khi va chạm
    }

    private void Update()
    {
        // --- Di chuyển ngang ---
        float horizontal = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);

        // --- Lật mặt nhân vật ---
        if (horizontal != 0)
            transform.localScale = new Vector3(Mathf.Sign(horizontal), 1, 1);

        // --- Nhảy ---
        if (Input.GetButtonDown("Jump"))
        {
            lastJumpPressedTime = Time.time;
            Jump();
        }
        GroundCheck();

        // Kiểm tra thời gian hợp lệ để nhảy (Coyote + Buffer)
        if (Time.time - lastJumpPressedTime <= jumpBufferTime && Time.time - lastGroundedTime <= coyoteTime)
            Jump();
    }

    private void GroundCheck()
    {
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundBoxSize, 0f, groundLayer);

        if (isGrounded)
            lastGroundedTime = Time.time;
    }

    private void Jump()
    {
        // reset trạng thái nhảy
        lastJumpPressedTime = -1;
        lastGroundedTime = -1;

        // Xóa vận tốc Y để nhảy ổn định
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundBoxSize);
    }
}
