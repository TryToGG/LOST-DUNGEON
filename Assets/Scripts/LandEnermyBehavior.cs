using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandEnemyBehavior : MonoBehaviour
{
    [Header("Assign(Required)")]
    public Rigidbody2D rb;
    [Space]
    [Header("Movement")]
    public float speed = 2f;
    public int Facing { get; private set; } = 1;
    [Space]
    [Header("Wall & Cliff Detection")]
    public float cliffCheckDistance = 0.5f;
    public float wallCheckDistance = 0.5f;
    public LayerMask groundLayer; // 在Inspector中设置地面层级
    [Space]
    [Header("Jump")]
    public float jumpForce = 5f;
    public int availableJumps = 2;
    public int currentAvailableJumps = 2;
    public float additionalJumpForce = 4f;
    public bool timeToJump = false;
    [Space]
    [Header("Other")]
    public int noUse = 1;

    private float moveDirection = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // 自动设置地面层级（如果未设置）
        if (groundLayer == 0)
        {
            groundLayer = LayerMask.GetMask("Default");
        }
    }

    bool IsAtCliffEdge()
    {
        // 计算检测起点（角色底部前方）
        Vector2 checkOrigin = (Vector2)transform.position + 
                             new Vector2(moveDirection * GetComponent<Collider2D>().bounds.extents.x, 
                                        -GetComponent<Collider2D>().bounds.extents.y);
        
        // 向下发射射线检测地面
        RaycastHit2D hit = Physics2D.Raycast(
            checkOrigin, 
            Vector2.down, 
            cliffCheckDistance, 
            groundLayer
        );
        
        // 如果没有检测到地面，说明是悬崖
        return hit.collider == null;
    }

    bool IsTouchingWall()
    {
        // 计算检测起点（角色前方）
        Vector2 checkOrigin = (Vector2)transform.position + 
                             new Vector2(moveDirection * GetComponent<Collider2D>().bounds.extents.x, 0);
        
        // 向前方发射射线检测墙壁
        RaycastHit2D hit = Physics2D.Raycast(
            checkOrigin, 
            Vector2.right * moveDirection, 
            wallCheckDistance, 
            groundLayer
        );
        
        // 如果检测到碰撞，说明有墙壁
        return hit.collider != null;
    }

    void CheckForObstacles()
    {
        // 如果遇到悬崖或者碰到墙壁，就掉头
        if (IsAtCliffEdge() || IsTouchingWall())
        {
            TurnAround();
        }
    }

    void TurnAround()
    {
        // 反转移动方向
        moveDirection *= -1;
        
        // 更新面向方向
        SetFacing((int)Mathf.Sign(moveDirection));
    }

    public void MovingBehavior()
    {
        // 检查障碍物
        CheckForObstacles();
        
        // 设置面向方向
        if (moveDirection != 0)
        {
            SetFacing(moveDirection > 0 ? 1 : -1);
        }
        
        // 应用移动
        rb.velocity = new Vector2(moveDirection * speed, rb.velocity.y);

        // 跳跃逻辑（如果需要）
        if (timeToJump && currentAvailableJumps == availableJumps)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            timeToJump = false;
        }
        else if (timeToJump && currentAvailableJumps != 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, additionalJumpForce);
            timeToJump = false;
        }
    }

    void Update()
    {
        MovingBehavior();
    }

    public void SetFacing(int dir)
    {
        if (dir == 0 || dir == Facing)
        {
            return;
        }
        Facing = dir;

        // 翻转敌人视觉
        var s = transform.localScale;
        s.x = Mathf.Abs(s.x) * dir;
        transform.localScale = s;
    }

    // 可视化调试
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        // 绘制悬崖检测线
        Vector2 cliffOrigin = (Vector2)transform.position + 
                             new Vector2(moveDirection * GetComponent<Collider2D>().bounds.extents.x, 
                                        -GetComponent<Collider2D>().bounds.extents.y);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(cliffOrigin, cliffOrigin + Vector2.down * cliffCheckDistance);
        
        // 绘制墙壁检测线
        Vector2 wallOrigin = (Vector2)transform.position + 
                            new Vector2(moveDirection * GetComponent<Collider2D>().bounds.extents.x, 0);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(wallOrigin, wallOrigin + Vector2.right * moveDirection * wallCheckDistance);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            currentAvailableJumps = availableJumps;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            currentAvailableJumps--;
        }
    }
}