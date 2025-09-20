using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttacks : MonoBehaviour
{
    [Header("Refrences")]
    public Transform firePoint;
    public GameObject attackPrefab;

    [Space]
    [Header("Attack Sets")]
    public float atkSpeed = 12f;
    public int damage = 10;
    public float cooldown = 0.25f;
    public float lifeTime = 0.8f;
    public bool canPierce = true;

    float cdTimer;
    PlayerBehavior character;

    private void Awake()
    {
        character = GetComponent<PlayerBehavior>();
    }
    // Update is called once per frame
    void Update()
    {
        cdTimer -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.J) && cdTimer <= 0f)
        {
            FireAttack();
            cdTimer = cooldown;
        }
    }

    void FireAttack()
    {
        GameObject go = Instantiate(attackPrefab, firePoint.position, Quaternion.identity);

        var rb = go.GetComponent<Rigidbody2D>();
        Vector2 dir = new Vector2(character.Facing, 0f);
        if (rb)
        {
            rb.velocity = dir.normalized * atkSpeed;
        }

        var atk = go.GetComponent<AtkProj>();
        if (atk)
        {
            atk.Setup(damage, dir, lifeTime, canPierce);
        }

        go.transform.right = dir;
    }
}
