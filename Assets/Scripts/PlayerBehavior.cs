using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.AI;
using UnityEngine.SceneManagement;

public class PlayerBehavior : MonoBehaviour
{
    [Header("Assign(Required)")]
    public Rigidbody2D rb;
    [SerializeField] private Transform firePoint;
    [Space]
    [Header("Jump")]
    public float jumpForce = 5f;
    [Space]
    public int availableJumps = 2;
    public int currentAvailableJumps = 2;
    public float additionalJumpForce = 4f;
    [Space]
    [Header("Walk")]
    public float speed = 1f;
    public int Facing { get; private set; } = 1;
    [Space]
    [Header("Attack")]
    public GameObject Attack;
    public float attackRangeVert = 1.5f;
    public float attackRangeHori = 0.5f;
    [Space]
    [Header("Status")]
    public int useless = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal"); //x-Axis Move
        float h = Input.GetAxisRaw("Horizontal");
        if (h != 0)
        {
            SetFacing(h > 0 ? 1 : -1);
        }
        rb.velocity = new Vector2(moveHorizontal * speed, rb.velocity.y);

        if (Input.GetKeyDown(KeyCode.Space) && currentAvailableJumps == availableJumps)  //y-Axis Move (jump)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
        else if (Input.GetKeyDown(KeyCode.Space) && currentAvailableJumps != 0)  //2nd (or more) jump
        {
            rb.velocity = new Vector2(rb.velocity.x, additionalJumpForce);
            currentAvailableJumps--;
        }
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

    //
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
            currentAvailableJumps --;
        }
    }
}
