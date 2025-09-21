using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandEnemyBehavior : MonoBehaviour
{
    [Header("Assign(Required)")]
    public Rigidbody2D rb;
    public Animator animator;  // 攻击动画控制

    [Header("Movement")]
    public float speed = 2f;
    public int Facing { get; private set; } = 1;
    private float moveDirection = 1f;

    [Header("Wall & Cliff Detection")]
    public float cliffCheckDistance = 0.5f;
    public float wallCheckDistance = 0.3f;
    public LayerMask groundLayer;
    public LayerMask Spike;

    [Header("Debug")]
    public bool showDebug = true;

    private bool isAttacking = false; // 是否在攻击状态

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (groundLayer == 0)
        {
            groundLayer = LayerMask.GetMask("Default");
        }
    }

    void FixedUpdate()
    {
        if (!isAttacking)  // 攻击时不走路
        {
            HandleMovement();
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y); // 停止水平移动
        }
    }

    void HandleMovement()
    {
        CheckForObstacles();

        SetFacing(moveDirection > 0 ? 1 : -1);

        rb.velocity = new Vector2(moveDirection * speed, rb.velocity.y);
    }

    #region Tilemap 检测
    bool IsAtCliffEdge()
    {
        Vector2 checkPos = (Vector2)transform.position + new Vector2(moveDirection * (GetComponent<Collider2D>().bounds.extents.x + 0.05f),
                                                                      -GetComponent<Collider2D>().bounds.extents.y - 0.05f);
        bool grounded = Physics2D.OverlapCircle(checkPos, 0.1f, groundLayer);

        if (showDebug)
            Debug.DrawLine(checkPos, checkPos + Vector2.down * 0.1f, Color.red);

        return !grounded;
    }

    bool IsTouchingWall()
    {
        Collider2D col = GetComponent<Collider2D>();

        Vector2 midOrigin = (Vector2)transform.position + Vector2.up * (col.bounds.size.y * 0.25f);
        Vector2 midSize = new Vector2(0.1f, col.bounds.size.y * 0.5f);

        Vector2 lowOrigin = (Vector2)transform.position + Vector2.down * (col.bounds.size.y * 0.25f);
        Vector2 lowSize = new Vector2(0.1f, col.bounds.size.y * 0.2f);

        RaycastHit2D wallHit = Physics2D.BoxCast(midOrigin, midSize, 0f, Vector2.right * moveDirection, wallCheckDistance, groundLayer);
        RaycastHit2D spikeHit = Physics2D.BoxCast(lowOrigin, lowSize, 0f, Vector2.right * moveDirection, wallCheckDistance, Spike);

        if (showDebug)
        {
            Color wallColor = wallHit.collider ? Color.red : Color.blue;
            Color spikeColor = spikeHit.collider ? Color.magenta : Color.cyan;

            Debug.DrawLine(midOrigin, midOrigin + Vector2.right * moveDirection * wallCheckDistance, wallColor);
            Debug.DrawLine(lowOrigin, lowOrigin + Vector2.right * moveDirection * wallCheckDistance, spikeColor);
        }

        return wallHit.collider != null || spikeHit.collider != null;
    }

    void CheckForObstacles()
    {
        if (IsAtCliffEdge() || IsTouchingWall())
        {
            TurnAround();
        }
    }
    #endregion

    void TurnAround()
    {
        moveDirection *= -1;
    }

    public void SetFacing(int dir)
    {
        if (dir == 0 || dir == Facing) return;
        Facing = dir;
        var s = transform.localScale;
        s.x = Mathf.Abs(s.x) * dir;
        transform.localScale = s;
    }

    // 碰到玩家时触发攻击
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            isAttacking = true;
            rb.velocity = Vector2.zero; // 停止运动

            if (animator != null)
            {
                animator.SetTrigger("Attack"); // 播放攻击动画
            }
        }
    }

    // 玩家离开后恢复巡逻
    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            isAttacking = false;
        }
    }
}
