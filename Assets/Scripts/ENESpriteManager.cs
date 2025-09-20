using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpriteAnimator : MonoBehaviour
{
    public Sprite[] walkSprites;
    public Sprite[] attackSprites;
    public float animationSpeed = 0.1f;
    
    private SpriteRenderer spriteRenderer;
    private int currentFrame = 0;
    private float timer = 0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= animationSpeed)
        {
            timer = 0f;
            currentFrame = (currentFrame + 1) % walkSprites.Length;
            spriteRenderer.sprite = walkSprites[currentFrame];
        }
    }
}