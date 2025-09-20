using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar2D : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("血条系统")]
    public RectTransform barContainer;

    [Tooltip("红条")]
    public Image fill;

    [Tooltip("黄条")]
    public Image yellowBar;

    [Tooltip("血条背景")]
    public Image bg;

    [Header("血条状态")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("黄条设置")]
    [Tooltip("扣血后黄条开始收缩前的停顿秒数")]
    public float yellowBarDelay = 0.2f;

    [Tooltip("黄条收缩速度")]
    public float yellowBarSpeed = 0.6f;

    [Header("血条震动")]
    public bool enableShake = true;
    public float shakeDuration = 0.12f;
    public float shakeMagnitude = 7f;

    [Header("火焰")]
    [Tooltip("粒子挂点")]
    public Transform flameFX;

    [Tooltip("粒子系统")]
    public ParticleSystem flamePS;

    [Tooltip("发射带厚度")]
    public float topBandHeight = 0f;

    [Tooltip("视为无血的阈值（≤该比例时不发射并清空粒子）")]
    public float noFireThreshold = 0.01f;

    [Tooltip("低血→火更旺（高发射率&更快上升）；高血→更弱")]
    public Vector2 emissionRateRange = new Vector2(90f, 20f); // x=低血时，y=满血时

    [Tooltip("粒子上升速度范围（y 方向），x=低血时，y=满血时")]
    public Vector2 riseSpeedRange = new Vector2(2.0f, 1.2f);

    RectTransform _selfRT;
    Vector2 _initialPos;
    float _fullWidth;
    float _yellowTarget;
    float _yellowTimer;

    private void Awake()
    {
        _selfRT = (RectTransform)transform;
        _initialPos = _selfRT.anchoredPosition;

        _fullWidth = barContainer ? barContainer.rect.width : 0f;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        float t = Ratio();
        if (fill) fill.fillAmount = t;
        if (yellowBar) yellowBar.fillAmount = t;
        UpdateFlameStripForFill(t, true);
        ApplyFlameIntensity(t);
    }
    public void SetMax(float max)
    {
        maxHealth = Mathf.Max(1f, max);
        SetHealth(currentHealth, false);
    }

    public void SetHealth(float value, bool playShakeOnDamage = true)
    {
        float prevT = Ratio();
        currentHealth = Mathf.Clamp(value, 0f, maxHealth);
        float t = Ratio();

        if (fill) fill.fillAmount = t;

        UpdateFlameStripForFill(t, false);
        ApplyFlameIntensity(t);

        if (yellowBar)
        {
            if (t < prevT)
            {
                _yellowTarget = t;
                _yellowTimer = yellowBarDelay;

                if (enableShake && playShakeOnDamage)
                    StartCoroutine(Shake());
            }
            else // 回血：黄条直接跟上
            {
                yellowBar.fillAmount = t;
                _yellowTarget = t;
                _yellowTimer = 0f;
            }
        }
    }

    public void Damage(float amount) => SetHealth(currentHealth - Mathf.Abs(amount), true);
    public void Heal(float amount) => SetHealth(currentHealth + Mathf.Abs(amount), false);

    void Update()
    {
        // 黄条延迟收缩追随
        if (yellowBar && yellowBar.fillAmount > _yellowTarget)
        {
            if (_yellowTimer > 0f) _yellowTimer -= Time.deltaTime;
            else
            {
                yellowBar.fillAmount = Mathf.MoveTowards(
                    yellowBar.fillAmount, _yellowTarget, yellowBarSpeed * Time.deltaTime);
            }
        }
    }

    void UpdateFlameStripForFill(float t, bool initialize)
    {
        if (!flamePS || !barContainer) return;

        var emission = flamePS.emission;
        var shape = flamePS.shape;
        shape.shapeType = ParticleSystemShapeType.Box;

        // 空血或近似空血：停止发射 & 清空现有粒子
        if (t <= noFireThreshold || _fullWidth <= 0.0001f)
        {
            emission.enabled = false;
            if (!initialize) flamePS.Clear(true);
            shape.scale = new Vector3(0f, 0f, 0f);
            return;
        }

        emission.enabled = true;

        // 当前“有血宽度”
        float curWidth = _fullWidth * t;

        // 顶部细带：宽=curWidth，高=topBandHeight
        shape.scale = new Vector3(curWidth, topBandHeight, 0f);

        if (flameFX)
        {
            Vector3 p = flameFX.localPosition;
            p.x = -_fullWidth * 0.5f + curWidth * 0.5f;
            flameFX.localPosition = p;
        }
    }

    // 粒子强度/上升速度随血量变化（低血更旺）
    void ApplyFlameIntensity(float t)
    {
        if (!flamePS) return;

        // 发射率：t=0 → emissionRateRange.x（旺）; t=1 → emissionRateRange.y（弱）
        var emission = flamePS.emission;
        float rate = Mathf.Lerp(emissionRateRange.x, emissionRateRange.y, t);
        emission.rateOverTime = rate;

        // 上升速度（Velocity over Lifetime 的 y）
        var vel = flamePS.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        float rise = Mathf.Lerp(riseSpeedRange.x, riseSpeedRange.y, t);
        vel.y = new ParticleSystem.MinMaxCurve(rise);
    }

    float Ratio() => (maxHealth > 0f) ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;

    IEnumerator Shake()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            Vector2 offset = Random.insideUnitCircle * shakeMagnitude;
            _selfRT.anchoredPosition = _initialPos + offset;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        _selfRT.anchoredPosition = _initialPos;
    }
}
