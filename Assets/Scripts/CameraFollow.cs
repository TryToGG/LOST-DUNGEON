using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("跟随目标")]

    [Tooltip("目标对象")]
    public Transform Target;

    [Tooltip("目标刚体")]
    public Rigidbody2D TargetRB;

    [Space]
    [Header("移动死区")]

    [Tooltip("死区大小")]
    public Vector2 DeadZoneSize = new Vector2(3f, 2f);

    [Tooltip("死区垂直偏移")]
    public float VerticalDZOffset = 0.5f;

    [Space]
    [Header("角色移动预判")]

    [Tooltip("X轴预判偏移")]
    public float LookAheadX = 2.0f;

    [Tooltip("预判阈值")]
    public float LookAheadThreshold = 0.1f;

    [Tooltip("预判返回速度")]
    public float LookAheadSpeed = 3f;

    [Space]
    [Header("移动阻尼")]

    [Tooltip("X轴")]
    public float DampingX = 0.15f;

    [Tooltip("Y轴")]
    public float DampingY = 0.15f;

    [Tooltip("最高速度")]
    public float MaxSpeed = 100f;

    [Space]
    [Header("垂直移动修正")]

    [Tooltip("下落额外系数")]
    public float FallFollowBoost = 0.3f;

    [Tooltip("下落速度阈值")]
    public float FallVelocityThreshold = -6f;

    [Space]
    [Header("地图边界")]

    [Tooltip("地图边框碰撞箱")]
    public Collider2D BoundryCollider;

    [Tooltip("手动边界")]
    public Vector2 ManualBoundsMin;

    [Tooltip("手动边界")]
    public Vector2 ManualBoundsMax;

    [Tooltip("相机缩放")]
    public bool AllowZoneOrthoSize = true;

    [Tooltip("相机缩放过渡速度")]
    public float OrthoLerpSpeed = 3f;

    [Tooltip("相机缩放默认大小")]
    public float DefaultOrthoSize = 5f;

    [Tooltip("相机抖动")]
    public float ShakeDecay = 5f;

<<<<<<< Updated upstream
=======
    [Tooltip("相机过渡")]
    public float TransitionDuration = 0.35f;

    [Tooltip("插值比例")]
    [Range(0f, 2f)] public float BoundsLerpWeight = 1.0f;

    public AnimationCurve TransitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

>>>>>>> Stashed changes
    Camera cam;
    Vector3 vel;
    float currentLookAheadX;
    Bounds globalBounds;
    Vector2 shakeOffset;
    float shakePower;

    readonly List<CameraZone2D> _zones = new List<CameraZone2D>();
    CameraZone2D _activeZone;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        RebuildGlobalBounds();
    }

    private void LateUpdate()
    {
        CameraParams p = BuildParamsFromActiveZone();

<<<<<<< Updated upstream
=======
        if (_isTransitioning)
        {
            _transitionT += (TransitionDuration <= 0f ? 1f : Time.deltaTime / TransitionDuration);
            float t = Mathf.Clamp01(_transitionT);
            float e = TransitionCurve != null ? TransitionCurve.Evaluate(t) : t;

            var p = CameraParams.Lerp(_fromParams, _toParams, e);

            Bounds clampBounds;
            if (BoundsLerpWeight > 0f)
            {
                Vector3 c = Vector3.Lerp(_fromBounds.center, _toBounds.center, e * BoundsLerpWeight);
                Vector3 s = Vector3.Lerp(_fromBounds.size, _toBounds.size, e * BoundsLerpWeight);
                clampBounds = new Bounds(c, s);
            }
            else
            {
                clampBounds = _toBounds;
            }

            if (AllowZoneOrthoSize && _toZone && _toZone.UseOrthoSize)
            {
                cam.orthographicSize = Mathf.Lerp(_fromOrtho, Mathf.Max(0.01f, _toOrtho), e);
            }

            ApplyCamera(p, clampBounds);
            if (t >= 1f)
            {
                _isTransitioning = false;
            }
            return;
        }
        ApplyCamera(targetParams, targetBounds);
        
        if (AllowZoneOrthoSize)
        {
            float want = (_activeZone && _activeZone.UseOrthoSize) ? _activeZone.OrthoSize : DefaultOrthoSize;
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, Mathf.Max(0.01f, want), OrthoLerpSpeed * Time.deltaTime);
        }
    }
    void ApplyCamera(CameraParams p, Bounds clampBounds)
    {
>>>>>>> Stashed changes
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        Vector3 camPos = transform.position;
        Vector2 camCenter = new Vector2(camPos.x, camPos.y);
        Vector2 deadHalf = p.deadZoneSize * 0.5f;

        Rect deadRect = new Rect(camCenter.x - deadHalf.x,camCenter.y - deadHalf.y + p.vertDeadCenterOffset,p.deadZoneSize.x,p.deadZoneSize.y);

        Vector2 targetPos = Target.position;
        Vector2 desiredCenter = camCenter;

        if (targetPos.x < deadRect.xMin)
        {
            desiredCenter.x -= (deadRect.xMin - targetPos.x);
        }
        else if (targetPos.x > deadRect.xMax)
        {
            desiredCenter.x += (targetPos.x - deadRect.xMax);
        }

        if (targetPos.y < deadRect.yMin)
        {
            desiredCenter.y -= (deadRect.yMin - targetPos.y);
        }
        else if (targetPos.y > deadRect.yMax)
        {
            desiredCenter.y += (targetPos.y - deadRect.yMax);
        }

        float speedX = TargetRB.velocity.x;
        float dir = Mathf.Sign(speedX);

        if (Mathf.Abs(speedX) > p.lookAheadThreshold)
        {
            currentLookAheadX = Mathf.Lerp(currentLookAheadX, dir * p.lookAheadX, Time.deltaTime * 4f);
        }
        else
        {
            currentLookAheadX = Mathf.MoveTowards(currentLookAheadX, 0f, p.lookAheadReturn * Time.deltaTime);
        }

        desiredCenter.x += currentLookAheadX;

        float dampingY = p.dampingY;
        float vy = TargetRB.velocity.y;
        if (vy < FallVelocityThreshold) dampingY = Mathf.Max(0.5f, p.dampingY - FallFollowBoost);

        float newX = Mathf.SmoothDamp(camPos.x, desiredCenter.x, ref vel.x, Mathf.Max(0.01f, p.dampingX), p.maxSpeed, Time.deltaTime);
        float newY = Mathf.SmoothDamp(camPos.y, desiredCenter.y, ref vel.y, Mathf.Max(0.01f, dampingY), p.maxSpeed, Time.deltaTime);
        Vector3 smoothed = new Vector3(newX, newY, camPos.z);

        Bounds clampBounds = _activeZone && _activeZone.HasBounds ? _activeZone.GetWorldBounds() : globalBounds;

        if (clampBounds.size != Vector3.zero)
        {
            float minX = clampBounds.min.x + halfW;
            float maxX = clampBounds.max.x - halfW;
            float minY = clampBounds.min.y + halfH;
            float maxY = clampBounds.max.y - halfH;

            if (minX > maxX) { float m = (minX + maxX) * 0.5f; minX = maxX = m; }
            if (minY > maxY) { float m = (minY + maxY) * 0.5f; minY = maxY = m; }

            smoothed.x = Mathf.Clamp(smoothed.x, minX, maxX);
            smoothed.y = Mathf.Clamp(smoothed.y, minY, maxY);
        }

        if (shakePower > 0f)
        {
            shakeOffset = Random.insideUnitCircle * shakePower;
            shakePower = Mathf.MoveTowards(shakePower, 0f, ShakeDecay * Time.deltaTime);
        }
        else shakeOffset = Vector2.zero;

        transform.position = smoothed + (Vector3)shakeOffset;

        if (AllowZoneOrthoSize)
        {
            float targetOrtho = _activeZone && _activeZone.UseOrthoSize ? _activeZone.OrthoSize : cam.orthographicSize;
            if (_activeZone && _activeZone.UseOrthoSize)
                cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, Mathf.Max(0.01f, targetOrtho), OrthoLerpSpeed * Time.deltaTime);
        }
    }
    void RebuildGlobalBounds()
    {
        if (BoundryCollider)
            globalBounds = BoundryCollider.bounds;
        else
        {
            var min = new Vector3(ManualBoundsMin.x, ManualBoundsMin.y, 0f);
            var max = new Vector3(ManualBoundsMax.x, ManualBoundsMax.y, 0f);
            if (max.x <= min.x || max.y <= min.y) globalBounds = new Bounds();
            else
            {
                Vector3 size = max - min;
                globalBounds = new Bounds((min + max) * 0.5f, size);
            }
        }
    }
    CameraParams BuildParamsFromActiveZone()
    {
        var p = new CameraParams
        {
            deadZoneSize = DeadZoneSize,
            vertDeadCenterOffset = VerticalDZOffset,
            lookAheadX = LookAheadX,
            lookAheadThreshold = LookAheadThreshold,
            lookAheadReturn = LookAheadSpeed,
            dampingX = DampingX,
            dampingY = DampingY,
            maxSpeed = MaxSpeed
        };

        if (_activeZone)
        {
            if (_activeZone.OverrideDeadZone) { p.deadZoneSize = _activeZone.DeadZoneSize; p.vertDeadCenterOffset = _activeZone.VerticalDeadZoneOffset; }
            if (_activeZone.OverrideLookAhead) { p.lookAheadX = _activeZone.LookAheadX; p.lookAheadThreshold = _activeZone.LookAheadThreshold; p.lookAheadReturn = _activeZone.LookAheadReturn; }
            if (_activeZoneOverrideDamping) { p.dampingX = _activeZone.DampingX; p.dampingY = _activeZone.DampingY; }
            if (_activeZoneOverrideMaxSpeed) { p.maxSpeed = _activeZone.MaxSpeed; }
        }
        return p;
    }
    internal void RegisterZone(CameraZone2D zone)
    {
        if (!_zones.Contains(zone)) _zones.Add(zone);
        PickActiveZone();
    }
    internal void UnregisterZone(CameraZone2D zone)
    {
        _zones.Remove(zone);
        PickActiveZone();
    }
    void PickActiveZone()
    { 
        CameraZone2D best = null;
        for (int i = 0; i < _zones.Count; i++)
        {
            var z = _zones[i];
            if (!best || z.Priority > best.Priority || (z.Priority == best.Priority && z.EnterFrame > best.EnterFrame))
            {
                best = z;
            }
        }
<<<<<<< Updated upstream
        _activeZone = best;
=======
        if (old != best)
        {
            BeginZoneTransition(old, best);
            _activeZone = best;
        }
    }
    void BeginZoneTransition(CameraZone2D from, CameraZone2D to)
    {
        _fromZone = from;
        _toZone = to;

        _fromParams = BuildParamsFromZone(from);
        _toParams = BuildParamsFromZone(to);

        _fromBounds = (from && from.HasBounds) ? from.GetWorldBounds() : globalBounds;
        _toBounds = (to && to.HasBounds) ? to.GetWorldBounds() : globalBounds;

        _fromOrtho = cam.orthographicSize;
        _toOrtho = (AllowZoneOrthoSize && to && to.UseOrthoSize) ? to.OrthoSize : DefaultOrthoSize;

        _transitionT = 0f;
        _isTransitioning = true;
    }

    void RebuildGlobalBounds()
    {
        if (BoundryCollider)
            globalBounds = BoundryCollider.bounds;
        else
        {
            var min = new Vector3(ManualBoundsMin.x, ManualBoundsMin.y, 0f);
            var max = new Vector3(ManualBoundsMax.x, ManualBoundsMax.y, 0f);
            if (max.x <= min.x || max.y <= min.y) globalBounds = new Bounds();
            else
            {
                Vector3 size = max - min;
                globalBounds = new Bounds((min + max) * 0.5f, size);
            }
        }
>>>>>>> Stashed changes
    }

    public void AddShake(float power) => shakePower = Mathf.Max(shakePower, power);

    struct CameraParams
    {
        public Vector2 deadZoneSize;
        public float vertDeadCenterOffset;
        public float lookAheadX, lookAheadThreshold, lookAheadReturn;
        public float dampingX, dampingY, maxSpeed;
    }

    bool _activeZoneOverrideDamping => _activeZone && _activeZone.OverrideDamping;
    bool _activeZoneOverrideMaxSpeed => _activeZone && _activeZone.OverrideMaxSpeed;

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!cam) cam = GetComponent<Camera>();
        var b = (_activeZone && _activeZone.HasBounds) ? _activeZone.GetWorldBounds() : globalBounds;
        Gizmos.color = Color.cyan;
        if (b.size != Vector3.zero) Gizmos.DrawWireCube(b.center, b.size);

        // 画默认死区
        Vector2 deadHalf = DeadZoneSize * 0.5f;
        Vector3 c = Application.isPlaying ? (Vector3)transform.position : transform.position;
        Rect r = new Rect(c.x - deadHalf.x, c.y - deadHalf.y + VerticalDZOffset, DeadZoneSize.x, DeadZoneSize.y);
        Gizmos.color = new Color(1, 1, 0, 0.5f);
        Gizmos.DrawWireCube(r.center, r.size);
    }
#endif
}
