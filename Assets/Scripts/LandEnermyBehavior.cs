using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandEnemyBehavior : MonoBehaviour
{
    [Header("Assign(Required)")]
    public Rigidbody2D rb;
    
    [Header("Movement")]
    public float speed = 2f;
    public int Facing { get; private set; } = 1;
    private float moveDirection = 1f;

    [Header("Wall & Cliff Detection")]
    public float cliffCheckDistance = 0.5f;
    public float wallCheckDistance = 0.5f;
    public LayerMask groundLayer;

    [Header("Jump")]
    public float jumpForce = 5f;
    public float additionalJumpForce = 4f;
    public int availableJumps = 2;
    private int currentAvailableJumps;
    private bool timeToJump = false;

    [Header("Player Detection")]
    public Transform player;           // 玩家 Transform
    public float detectDistance = 3f;  // 前方检测玩家距离

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (groundLayer == 0)
        {
            groundLayer = LayerMask.GetMask("Default");
        }

        currentAvailableJumps = availableJumps;
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        // 检查障碍物并可能掉头
        CheckForObstacles();

        // 前方有玩家则触发跳跃
        if (IsPlayerAhead() && currentAvailableJumps > 0)
        {
            timeToJump = true;
        }

        // 设置面向方向
        SetFacing(moveDirection > 0 ? 1 : -1);

        // 应用水平移动
        rb.velocity = new Vector2(moveDirection * speed, rb.velocity.y);

        // 跳跃逻辑
        if (timeToJump && currentAvailableJumps > 0)
        {
            float force = (currentAvailableJumps == availableJumps) ? jumpForce : additionalJumpForce;
            rb.velocity = new Vector2(rb.velocity.x, force);
            currentAvailableJumps--;
            timeToJump = false;
        }

        // 重置跳跃次数（落地检测）
        if (IsGrounded())
        {
            currentAvailableJumps = availableJumps;
        }
    }

    bool IsAtCliffEdge()
    {
        Vector2 checkOrigin = (Vector2)transform.position +
                              new Vector2(moveDirection * GetComponent<Collider2D>().bounds.extents.x,
                                          -GetComponent<Collider2D>().bounds.extents.y);
        RaycastHit2D hit = Physics2D.Raycast(checkOrigin, Vector2.down, cliffCheckDistance, groundLayer);
        return hit.collider == null;
    }

    bool IsTouchingWall()
    {
        Vector2 checkOrigin = (Vector2)transform.position +
                              new Vector2(moveDirection * GetComponent<Collider2D>().bounds.extents.x, 0);
        RaycastHit2D hit = Physics2D.Raycast(checkOrigin, Vector2.right * moveDirection, wallCheckDistance, groundLayer);
        return hit.collider != null;
    }

    void CheckForObstacles()
    {
        if (IsAtCliffEdge() || IsTouchingWall())
        {
            TurnAround();
        }
    }

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

    bool IsGrounded()
    {
        Vector2 origin = (Vector2)transform.position + Vector2.down * (GetComponent<Collider2D>().bounds.extents.y + 0.1f);
        return Physics2D.Raycast(origin, Vector2.down, 0.2f, groundLayer);
    }

    bool IsPlayerAhead()
    {
        if (player == null) return false;

        Vector2 origin = (Vector2)transform.position;
        Vector2 dir = Vector2.right * moveDirection;

        RaycastHit2D hit = Physics2D.Raycast(origin, dir, detectDistance, LayerMask.GetMask("Player"));

        Debug.DrawLine(origin, origin + dir * detectDistance, Color.yellow);

        return hit.collider != null;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // 悬崖检测线
        Vector2 cliffOrigin = (Vector2)transform.position +
                              new Vector2(moveDirection * GetComponent<Collider2D>().bounds.extents.x,
                                          -GetComponent<Collider2D>().bounds.extents.y);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(cliffOrigin, cliffOrigin + Vector2.down * cliffCheckDistance);

        // 墙壁检测线
        Vector2 wallOrigin = (Vector2)transform.position +
                             new Vector2(moveDirection * GetComponent<Collider2D>().bounds.extents.x, 0);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(wallOrigin, wallOrigin + Vector2.right * moveDirection * wallCheckDistance);

        // 玩家检测线
        if (player != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine((Vector2)transform.position, (Vector2)transform.position + Vector2.right * moveDirection * detectDistance);
        }
    }
}
