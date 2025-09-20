using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtkProj : MonoBehaviour
{
    [SerializeField] LayerMask hitMask;
    [SerializeField] LayerMask blockMask;

    int _damage;
    Vector2 _dir;
    float _life;
    bool _pierce;

    float _timer;
    bool _consumed;

    public void Setup(int damage, Vector2 dir, float lifeTime, bool canPierce)
    { 
        _damage = damage;
        _dir = dir;
        _life = lifeTime;
        _pierce = canPierce;
        _timer = 0f;
    }

    private void OnEnable()
    {
        _timer = 0f;
        _consumed = false;
    }

    private void Update()
    {
        if (_life > 0f)
        {
            _timer += Time.deltaTime;
            if (_timer >= _life && !_consumed)
            {
                Debug.Log("Overtime And Destroy");
                Kill();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_consumed)
        {
            return;
        }
        int otherLayer = collision.gameObject.layer;
        if ((hitMask.value & (1 << otherLayer)) != 0)
        {
            var dmg = collision.GetComponent<IDamageable>();
            if (dmg != null)
            {
                dmg.TakeDamage(_damage);
            }
            if (!_pierce)
            { 
                _consumed = true;
                Kill();
            }
            return;
        }

        if ((blockMask.value & (1 << otherLayer)) != 0)
        {
            if (!_pierce)
            {
                _consumed = true;
                Kill();
            }
            return;
        }
    }

    void Kill()
    { 
        Destroy(gameObject);
    }
}
