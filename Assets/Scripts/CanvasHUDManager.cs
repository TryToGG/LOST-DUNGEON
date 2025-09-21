using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasHUDManager : MonoBehaviour
{
    public Camera cam;
    public RectTransform canvasRect;                         
    public Vector2 referenceResolution = new Vector2(1920, 1080);
    [Range(0.01f, 1f)] public float screenHeightPercent = 0.2f;
    public Vector2 viewportAnchor = new Vector2(0f, 1f);    
    public Vector2 pixelOffset = new Vector2(40f, -40f);   

    public float planeDistance = 5f;

    public float scale = 1;

    void Reset()
    {
        scale = 1;
        cam = Camera.main;
        canvasRect = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (!cam) cam = Camera.main;
        if (!canvasRect || !cam) return;

        float worldScreenHeight;
        if (cam.orthographic)
        {
            worldScreenHeight = 2f * cam.orthographicSize * screenHeightPercent;
        }
        else
        {
            float frustumHeight = 2f * planeDistance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            worldScreenHeight = frustumHeight * screenHeightPercent;
        }

        scale = worldScreenHeight / referenceResolution.y;
        transform.localScale = Vector3.one * scale;

        Vector3 vp = new Vector3(viewportAnchor.x, viewportAnchor.y,
            cam.orthographic ? Mathf.Abs(cam.transform.position.z - transform.position.z) : planeDistance);

        Vector3 worldPos = cam.ViewportToWorldPoint(vp);

        Vector2 worldOffset = pixelOffset * scale;
        Vector3 right = cam.transform.right;
        Vector3 up = cam.transform.up;

        transform.position = worldPos + right * worldOffset.x + up * worldOffset.y;
        transform.rotation = cam.transform.rotation;
    }
}
