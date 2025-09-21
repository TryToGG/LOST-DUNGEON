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
    public float maxHealth = 100;
    public float currentHealth = 100;

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

    [Tooltip("粒子画布适配")]
    public CanvasHUDManager hud;

    [Tooltip("粒子大小修正")]
    public float particleMultiplier = 1f;

    [Space]
    [Header("受伤冷却")]

    [Tooltip("是否启用")]
    public bool useDamageCooldown = true;
    [Tooltip("冷却时间")]
    public float damageCooldown = 0.5f;

    [Space]
    [Header("金币收集")]

    [Tooltip("金币条贴图")]
    public Image imageCoin;

    [Tooltip("金币上限")]
    public int maxCoin = 60;

    [Tooltip("金币负面效果阈值")]
    public int thresholdCoinDebuff = 20;

    [Tooltip("负面效果系数")]
    public float debuffMultiplyer = 1f;

    [Tooltip("负面效果强度(%)")]
    public float debuffStrength = 5f;

    float _lastDamageTime = -999f;
    public bool IsOnDamageCooldown => useDamageCooldown && (Time.time < _lastDamageTime + damageCooldown);

    RectTransform _selfRT;
    Vector2 _initialPos;
    float _fullWidth;
    float _yellowTarget;
    float _yellowTimer;
    int _currentCoinAmount = 0;

    private void Awake()
    {
        SetCoin(_currentCoinAmount);

        _selfRT = (RectTransform)transform;
        _initialPos = _selfRT.anchoredPosition;

        _fullWidth = barContainer ? barContainer.rect.width : 0f;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        float t = Ratio();
        if (fill) fill.fillAmount = t;
        if (yellowBar) yellowBar.fillAmount = t;

        _yellowTarget = t;
        _yellowTimer = 0f;

        UpdateFlameStripForFill(t, true);
    }

    //True: 金库已满 False：金库未满
    public bool addCoin(int amount)
    {
        if (_currentCoinAmount + amount > 60)
        {
            return true;
        }
        _currentCoinAmount += amount;
        SetCoin(_currentCoinAmount);
        return false;
    }

    
    public void SetCoin(int value)
    {
        imageCoin.fillAmount = value * 0.016666667f;
        if (value > thresholdCoinDebuff)
        { 
            debuffMultiplyer = 1 - (value - thresholdCoinDebuff) * debuffStrength * 0.1f;
        }
    }

    public void SetMax(float max)
    {
        maxHealth = Mathf.Max(1, max);
        SetHealth(currentHealth, false);
    }

    public void SetHealth(float value, bool playShakeOnDamage = true)
    {
        bool tryingDamage = value < currentHealth;
        if (useDamageCooldown && tryingDamage && (Time.time < _lastDamageTime + damageCooldown))
        {
            return;
        }
        float prevT = Ratio();
        currentHealth = Mathf.Clamp(value, 0, maxHealth);
        float t = Ratio();

        if (fill) fill.fillAmount = t;

        UpdateFlameStripForFill(t, false);

        if (yellowBar)
        {
            if (t < prevT)
            {
                _lastDamageTime = Time.time;

                _yellowTarget = t;
                _yellowTimer = yellowBarDelay;

                if (enableShake && playShakeOnDamage)
                    StartCoroutine(Shake());
            }
            else
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

        if (t <= noFireThreshold || _fullWidth <= 0.0001f)
        {
            // 空血：停发并清空
            emission.enabled = false;
            if (!initialize) flamePS.Clear(true);
            shape.scale = new Vector3(0f, 0f, 0f);
            return;
        }

        emission.enabled = true;

        float curWidth = _fullWidth * t;
        shape.scale = new Vector3(curWidth * particleMultiplier * hud.scale, topBandHeight, 0f);

        if (flameFX)
        {
            Vector3 p = flameFX.localPosition;
            p.x = -_fullWidth * 0.5f + curWidth * 0.5f;
            flameFX.localPosition = p;
        }
    }

    float Ratio() => (maxHealth > 0f) ? currentHealth / maxHealth : 0f;

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
