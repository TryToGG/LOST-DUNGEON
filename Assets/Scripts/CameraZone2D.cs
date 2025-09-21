using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZone2D : MonoBehaviour
{
    [Header("相机优先级")]
    public int Priority = 0;
    [HideInInspector] public int EnterFrame;

    [Space]
    [Header("局部边框")]

    [Tooltip("启用局部边框")]
    public bool UseLocalBounds = true;

    [Tooltip("局部边框大小")]
    public Vector2 BoundsSize = new Vector2(20, 12);

    [Space]
    [Header("局部相机设置")]

    [Space]
    [Header("死区")]

    [Tooltip("启用局部死区")]
    public bool OverrideDeadZone = false;
    public Vector2 DeadZoneSize = new Vector2(3f, 2f);
    public float VerticalDeadZoneOffset = 0.5f;

    [Space]
    [Header("预判")]

    [Tooltip("启用局部预判")]
    public bool OverrideLookAhead = false;
    public float LookAheadX = 2f;
    public float LookAheadThreshold = 0.1f;
    public float LookAheadReturn = 3f;

    [Space]
    [Header("阻尼")]

    [Tooltip("启用局部阻尼")]
    public bool OverrideDamping = false;
    public float DampingX = 0.15f;
    public float DampingY = 0.25f;

    [Space]
    [Header("最高速度")]

    [Tooltip("启用局部最高速度")]
    public bool OverrideMaxSpeed = false;
    public float MaxSpeed = 100f;

    [Space]
    [Header("相机视野")]

    [Tooltip("启用局部相机视野")]
    public bool UseOrthoSize = false;
    public float OrthoSize = 5f;

    CameraFollow cam;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!cam)
        { 
            cam = Camera.main ? Camera.main.GetComponent<CameraFollow>() : null;
        }
        if (!cam)
        {
            return;
        }
        if (!collision.transform || !cam.Target)
        {
            return;
        }
        if (collision.transform != cam.Target)
        {
            return;
        }

        EnterFrame = Time.frameCount;
        cam.RegisterZone(this);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!cam)
        {
            cam = Camera.main ? Camera.main.GetComponent<CameraFollow>() : null;
        }
        if (!cam)
        {
            return;
        }
        if (!collision.transform || !cam.Target)
        {
            return;
        }
        if (collision.transform != cam.Target)
        {
            return;
        }

        cam.UnregisterZone(this);
    }

    public bool HasBounds => UseLocalBounds && BoundsSize.x > 0.1f && BoundsSize.y > 0.01f;

    public Bounds GetWorldBounds()
    {
        if (!HasBounds)
        { 
            return new Bounds();
        }
        var size = new Vector3(BoundsSize.x, BoundsSize.y, 0f);
        return new Bounds(transform.position, size);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (HasBounds)
        {
            Gizmos.color = new Color(0, 1, 1, 0.35f);
            Gizmos.DrawWireCube(transform.position, new Vector3(BoundsSize.x, BoundsSize.y, 0));
        }
    }
#endif
}
