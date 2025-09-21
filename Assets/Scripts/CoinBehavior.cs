using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinBehavior : MonoBehaviour
{
    public HealthBar2D healthBar2D;
    public GameObject player;

    public int coinValue = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            if (!healthBar2D.addCoin(coinValue))
            {
                Destroy(gameObject);
            }
        }
    }
}
