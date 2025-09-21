using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFind : MonoBehaviour
{
    public enum Mode { Loop, Point2}
    [Header("Path")]
    public Mode mode = Mode.Loop;
    public float defaultWait = 0f;

    public bool drawGizmos = true;
    public Color gizmoLine = new Color(1f, 0.7f, 0.2f, 0.9f);
    public Color gizmoPoint = new Color(0.2f, 0.9f, 1f, 0.9f);
    public float gizmoPointSize = 0.12f;

    [System.Serializable]
    public class Waypoint
    {
        public Transform t;
        public float waitOverride = -1f; // <0 ±íÊ¾ÓÃ defaultWait
    }

    public List<Waypoint> points = new List<Waypoint>();

    private void OnValidate()
    {
        if (points.Count == 0 && transform.childCount > 0)
        {
            points = new List<Waypoint>();
            for (int i = 0; i < transform.childCount; i++)
            {
                points.Add(new Waypoint { t = transform.GetChild(i) });
            }
        }
    }

    public int Count => points.Count;
    public Vector2 GetPos(int idx) => (points[idx].t ? (Vector2)points[idx].t.position : (Vector2)transform.position);
    public float GetWait(int idx)
    {
        float w = points[idx].waitOverride;
        return (w >= 0f) ? w : defaultWait;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!drawGizmos || Count == 0) return;
        Gizmos.color = gizmoLine;

        for (int i = 0; i < Count; i++)
        {
            Vector3 a = GetPos(i);
            Vector3 b = GetPos((i + 1) % Count);
            if (mode == Mode.Loop || i < Count - 1)
                Gizmos.DrawLine(a, b);
        }
        for (int i = 0; i < Count; i++)
        {
            Gizmos.color = gizmoPoint;
            Gizmos.DrawSphere(GetPos(i), gizmoPointSize);
#if UNITY_EDITOR
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(GetPos(i) + Vector2.up * (gizmoPointSize * 2f), $"#{i}");
#endif
        }
    }
#endif
}
