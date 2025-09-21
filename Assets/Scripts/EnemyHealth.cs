using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public float maxHP = 50;
    float _hp;
    public CameraFollow cam;
    public float shake = 0.25f;

    void Awake() => _hp = maxHP;

    public void TakeDamage(float amount)
    { 
        _hp -= amount;
        cam.AddShake(0.25f);
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
