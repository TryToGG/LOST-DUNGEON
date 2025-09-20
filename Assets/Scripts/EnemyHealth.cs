using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public float maxHP = 50;
    float _hp;

    void Awake() => _hp = maxHP;

    public void TakeDamage(float amount)
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
