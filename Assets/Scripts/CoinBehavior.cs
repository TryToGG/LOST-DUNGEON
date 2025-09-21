using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinBehavior : MonoBehaviour
{
    [Header("UI / Player")]
    public HealthBar2D healthBar2D;
    public GameObject player;

    [Header("Coin Settings")]
    public int coinValue = 1;

    [Header("Animation")]
    public Sprite[] coinSprites;   // 拖拽切好的金币帧
    public float frameRate = 0.1f; // 每帧切换时间（秒）

    private SpriteRenderer sr;
    private int currentFrame;
    private float timer;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        currentFrame = 0;
        timer = 0f;
        if (coinSprites.Length > 0)
            sr.sprite = coinSprites[0];
    }

    void Update()
    {
        HandleAnimation();
    }

    void HandleAnimation()
    {
        if (coinSprites.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= frameRate)
        {
            timer = 0f;
            currentFrame = (currentFrame + 1) % coinSprites.Length;
            sr.sprite = coinSprites[currentFrame];
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (!healthBar2D.addCoin(coinValue))
            {
                Destroy(gameObject);
            }
        }
    }
}
