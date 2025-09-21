using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchControl : MonoBehaviour
{
    public DoorController controller;

    public SpriteRenderer switchRenderer;

    [Header("开关-开 图")]
    public Sprite switchA;
    [Header("开关-关 图")]
    public Sprite switchB;
    [Space]

    public bool _ifOn;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("ATTACK"))
        {
            if (_ifOn)
            {
                _ifOn = false;
                controller.CloseTheDoor();
                switchRenderer.sprite = switchB;
            }
            else
            {
                _ifOn = true;
                controller.OpenTheDoor();
                switchRenderer.sprite = switchA;
            }
        }
    }
}
