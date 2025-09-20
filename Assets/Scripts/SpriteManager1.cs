using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpriteAnimator : MonoBehaviour
{
    public Sprite[] walkSprites;
    public Sprite[] attackSprites;
    public float animationSpeed = 0.1f;

    private SpriteRenderer spriteRenderer;
    private int currentFrame = 0;
    private float timer = 0f;

    void PlayTheAnimation(Sprite[] sprites)
    {
        timer += Time.deltaTime;
        if (timer >= animationSpeed)
        {
            timer = 0f;
            currentFrame = (currentFrame + 1) % walkSprites.Length;
            spriteRenderer.sprite = walkSprites[currentFrame];
        }
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (GetComponent<PlayerMovement>().isMoving)
        {
            PlayTheAnimation(walkSprites);
        }
        else
        {
            currentFrame = 0;
            timer = 0f;
            spriteRenderer.sprite = walkSprites[0];
        }
    }
    
}