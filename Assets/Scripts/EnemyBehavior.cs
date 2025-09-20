using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    public HealthBar2D healthBar2D;
    public GameObject player;

    public float atkDamage = 10;

    public bool canMove = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            Debug.Log("Take Damage");
            healthBar2D.Damage(atkDamage);
        }
    }
}
