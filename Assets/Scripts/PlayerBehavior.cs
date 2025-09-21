using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.SceneManagement;

public class PlayerBehavior : MonoBehaviour
{
    [Header("攻击系统")]

    [Tooltip("子弹/剑气射出点")]
    [SerializeField] private Transform firePoint;

    [Space]
    [Header("跳跃系统")]

    [Tooltip("跳跃力度")]
    public float jumpForce = 5f;

    [Tooltip("是否可以二段跳")]
    public bool allowDoubleJump = true;

    [Tooltip("额外重力系数")]
    public float fallGravityMultiplier = 2.5f;

    [Tooltip("松开跳跃重力系数")]
    public float lowJumpGravityMt = 2.0f;

    [Space]
    [Header("落地检测")]

    [Tooltip("地面图层")]
    public LayerMask groundMask;

    [Tooltip("检测距离")]
    public float groundCheckDist = 0.05f;

    [Tooltip("检测盒尺寸")]
    public Vector2 boxCastSize = Vector2.zero;

    [Tooltip("检测盒水平缩小量")]
    public Vector2 boxCastShrink = new Vector2(0.05f, 0f);

    [Space]
    [Header("移动系统")]

    [Tooltip("玩家移动速度系数")]
    public float speed = 1f;

    [Tooltip("玩家方向")]
    public int Facing { get; private set; } = 1;

    [Space]
    [Header("血条")]
    public HealthBar2D healthBar;

    [Space]
    [Header("输入")]
    
    [Tooltip("Jump键绑定")]
    public string jumpButton = "Space";

    //玩家刚体
    Rigidbody2D rb;

    //玩家碰撞箱
    Collider2D col;

    //是否按下跳跃键
    bool jumpHeld;

    //允许二段跳?
    bool canDoubleJump;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal"); //x-Axis Move
        float h = Input.GetAxisRaw("Horizontal");
        if (h != 0)
        {
            SetFacing(h > 0 ? 1 : -1);
        }
        rb.velocity = new Vector2(moveHorizontal * speed * healthBar.debuffMultiplyer, rb.velocity.y);

        bool jumpPressed = Input.GetButtonDown(jumpButton);
        jumpHeld = Input.GetButton(jumpButton);

        bool grounded = IsGrounded();

        if (grounded)
        {
            canDoubleJump = allowDoubleJump;
        }

        if (jumpPressed)
        {
            if (grounded)
            {
                DoJump();
            }
            else if (allowDoubleJump && canDoubleJump)
            {
                DoJump();
                canDoubleJump = false;
            }
        }
        ApplyJumpGravity();
    }

    void DoJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    void ApplyJumpGravity()
    {
        if (rb.velocity.y < -Mathf.Epsilon)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallGravityMultiplier - 1f) * Time.deltaTime;
        }
        else if (rb.velocity.y > -Mathf.Epsilon && !jumpHeld)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpGravityMt - 1f) * Time.deltaTime;
        }
    }

    bool IsGrounded()
    { 
        Bounds b = col.bounds;
        Vector2 size = new Vector2(b.size.x - boxCastShrink.x, Mathf.Max(0.01f, b.size.y - boxCastShrink.y));
        Vector2 origin = b.center;

        RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, Vector2.down, groundCheckDist, groundMask);
        return hit.collider != null;
    }

    public void SetFacing(int dir)
    {
        if (dir == 0 || dir == Facing)
        {
            return;
        }
        Facing = dir;

        var s = transform.localScale;
        s.x = Mathf.Abs(s.x) * dir;
        transform.localScale = s;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!col) col = GetComponent<Collider2D>();
        Bounds b = col.bounds;
        Vector2 size = new Vector2(b.size.x - boxCastShrink.x, Mathf.Max(0.01f, b.size.y - boxCastShrink.y));
        Vector2 origin = b.center + Vector3.down * groundCheckDist;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(origin, size);
    }
#endif
}
