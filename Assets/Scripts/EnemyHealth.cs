using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public int maxHP = 50;
    int _hp;

    void Awake() => _hp = maxHP;

    public void TakeDamage(int amount)
    { 
        _hp -= amount;
        if (_hp <= 0)
        {
            Die();
        }
    }

    void Die()
    { 
        // Animations
        Destroy(gameObject);
    }
}
