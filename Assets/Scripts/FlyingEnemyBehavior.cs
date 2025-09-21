using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

public class FlyingEnemyBehavior : MonoBehaviour
{
    public enum FSM { Patrol,Chase,Return}

    [Header("巡视设置")]

    public PathFind path;
    public int startIndex = 0;
    public float arriveRadius = 0.25f;
    public bool usePingPongFromPath = true;
    public bool faceVelocityFlipX = true;

    [Space]
    [Header("移动设置")]

    [Tooltip("巡航速度")]
    public float maxSpeed = 4.5f;
    [Tooltip("巡航加速度")]
    public float acceleration = 12f;
    [Tooltip("加速度刹车")]
    public float braking = 16f;
    [Tooltip("转弯辅助")]
    public float turnAssist = 0.5f;

    [Header("停留设置")]

    public bool hover = true;
    public float hoverAmplitude = 0.15f;
    public float hoverFrequency = 2f;

    [Header("追踪设置")]

    [Tooltip("角色绑定")]
    public Transform player;
    [Tooltip("仇恨半径")]
    public float aggroRadius = 6f;
    [Tooltip("仇恨消失半径")]
    public float loseRadius = 9f;
    [Tooltip("追击速度")]
    public float chaseSpeed = 6f;
    [Tooltip("追击加速度")]
    public float chaseAcceleration = 14f;
    [Tooltip("无意义（开发用")]
    public float minChaseTime = 0.4f;

    [Header("返回设置")]

    [Tooltip("无意义（开发用")]
    public float rejoinPathDist = 0.35f;
    [Tooltip("返回速度")]
    public float returnSpeed = 5f;
    [Tooltip("返回加速度")]
    public float returnAcceleration = 12f;

    [Header("路径逗留设置")]

    [Tooltip("路径点停留")]
    public bool waitAtPoint = true;
    private bool waiting;

    Rigidbody2D _rb;
    FSM _state = FSM.Patrol;
    int _idx;
    int _dir = +1;
    float _stateEnterTime;
    Vector2 _anchorHoverOffset;
    Vector2 _homePoint;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _idx = Mathf.Clamp(startIndex, 0, path.Count - 1);
        transform.position = path.GetPos(_idx);
        _anchorHoverOffset = Vector2.zero;
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }

    private void Update()
    {
        if (faceVelocityFlipX)
        {
            if (Mathf.Abs(_rb.velocity.x) > 0.02f)
            {
                Vector3 s = transform.localScale;
                s.x = Mathf.Abs(s.x) * Mathf.Sign(_rb.velocity.x);
                transform.localScale = s;
            }
        }

        if (hover)
        {
            float y = Mathf.Sin(Time.time * (Mathf.PI * 2f) * hoverFrequency) * hoverAmplitude;
            _anchorHoverOffset = new Vector2(0f, y);
        }
        else _anchorHoverOffset = Vector2.zero;

        Vector2 toPlayer = player ? ((Vector2)player.position - (Vector2)transform.position) : Vector2.positiveInfinity;
        float distPlayer = player ? toPlayer.magnitude : Mathf.Infinity;

        switch (_state)
        {
            case FSM.Patrol:
                if (player && distPlayer <= aggroRadius) SwitchState(FSM.Chase);
                break;

            case FSM.Chase:
                if (Time.time - _stateEnterTime >= minChaseTime && (!player || distPlayer >= loseRadius))
                    SwitchState(FSM.Return);
                break;

            case FSM.Return:
                // 回到路径最近点后切回巡逻
                if (Vector2.Distance(transform.position, _homePoint) <= rejoinPathDist)
                    SwitchState(FSM.Patrol);
                break;
        }
    }

    private void FixedUpdate()
    {
        switch (_state)
        {
            case FSM.Patrol: TickPatrol(); break;
            case FSM.Chase: TickChase(); break;
            case FSM.Return: TickReturn(); break;
        }
    }
    void SwitchState(FSM s)
    {
        _state = s;
        _stateEnterTime = Time.time;

        if (s == FSM.Return)
        {
            int nearest = _idx;
            float best = Mathf.Infinity;
            for (int i = 0; i < path.Count; i++)
            {
                float d = Vector2.Distance(transform.position, path.GetPos(i));
                if (d < best) { best = d; nearest = i; }
            }
            _idx = nearest;
            _homePoint = path.GetPos(_idx);
            waiting = false;
        }
    }

    void TickPatrol()
    {
        if (waiting) {_rb.velocity = Vector2.Lerp(_rb.velocity, Vector2.zero, 0.15f); return; }
        Vector2 target = path.GetPos(_idx) + _anchorHoverOffset;
        MoveTowards(target, maxSpeed, acceleration, braking);

        if (Vector2.Distance(transform.position, path.GetPos(_idx)) <= arriveRadius)
        {
            if (waitAtPoint)
            {
                float wait = path.GetWait(_idx);
                if (wait > 0f) StartCoroutine(CoWaitAtPoint(wait));
            }
            AdvanceIndex();
        }
    }
    IEnumerator CoWaitAtPoint(float t)
    {
        waiting = true;
        yield return new WaitForSeconds(t);
        waiting = false;
    }
    void AdvanceIndex()
    {
        if (!usePingPongFromPath || path.mode == PathFind.Mode.Loop)
        {
            _idx = (_idx + 1) % path.Count;
        }
        else
        {
            int next = _idx + _dir;
            if (next >= path.Count || next < 0)
            {
                _dir *= -1;
                next = Mathf.Clamp(_idx + _dir, 0, path.Count - 1);
            }
            _idx = next;
        }
    }
    void TickChase()
    {
        if (!player) { SwitchState(FSM.Return); return; }
        Vector2 target = (Vector2)player.position;
        MoveTowards(target, chaseSpeed, chaseAcceleration, braking * 0.6f);
    }
    void TickReturn()
    {
        Vector2 target = _homePoint + _anchorHoverOffset;
        MoveTowards(target, returnSpeed, returnAcceleration, braking);

        int nearest = _idx;
        float best = Vector2.Distance(transform.position, _homePoint);
        for (int i = 0; i < path.Count; i++)
        {
            float d = Vector2.Distance(transform.position, path.GetPos(i));
            if (d < best) { best = d; nearest = i; }
        }
        if (nearest != _idx) { _idx = nearest; _homePoint = path.GetPos(_idx); }
    }
    void MoveTowards(Vector2 target, float maxSpd, float accel, float brake)
    {
        Vector2 pos = _rb.position;
        Vector2 to = (target - pos);
        float dist = to.magnitude;
        Vector2 dirN = (dist > 1e-3f) ? (to / dist) : Vector2.zero;

        float slowRadius = Mathf.Max(arriveRadius * 1.5f, 0.01f);
        float t = Mathf.Clamp01(dist / slowRadius);
        float desiredSpeed = Mathf.Lerp(0f, maxSpd, t);
        Vector2 desiredVel = dirN * desiredSpeed;

        float alignment = Vector2.Dot(_rb.velocity.sqrMagnitude > 1e-6f ? _rb.velocity.normalized : Vector2.zero, dirN);
        float assist = Mathf.Clamp01((1f - Mathf.Max(alignment, 0f))) * turnAssist;
        float usedAccel = accel * (1f + assist);

        Vector2 dv = desiredVel - _rb.velocity;
        float a = (dv.sqrMagnitude > 1e-6f ? usedAccel : brake) * Time.fixedDeltaTime;
        Vector2 step = Vector2.ClampMagnitude(dv, a);
        _rb.velocity += step;
    }
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Color c = (_state == FSM.Chase) ? Color.red : (_state == FSM.Return ? Color.cyan : Color.yellow);
        Gizmos.color = c;
        Gizmos.DrawWireSphere(transform.position, arriveRadius);

        if (player)
        {
            Gizmos.color = new Color(1, 0, 0, 0.15f);
            Gizmos.DrawWireSphere(transform.position, aggroRadius);
            Gizmos.color = new Color(1, 0.4f, 0.2f, 0.15f);
            Gizmos.DrawWireSphere(transform.position, loseRadius);
        }
    }
#endif
}
