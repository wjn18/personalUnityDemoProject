using UnityEngine;
using UnityEngine.UI;
public class HealthBarUI : MonoBehaviour
{
    [Header("Refs")]
    public Image fillMain;     // 主血条
    public Image fillDelay;    // 延迟血条

    [Header("Target")]
    public EnemyRuntime enemy; // 绑定敌人

    [Header("Tuning")]
    public float delaySpeed = 1.5f;
    public float smoothSpeed = 12f;

    float target = 1f;
    float current = 1f;

    void Start()
    {
        if (enemy == null)
            enemy = GetComponentInParent<EnemyRuntime>();

        if (enemy != null)
        {
            enemy.OnHPChanged += OnEnemyHPChanged;

            // 初始化
            SetHealth(enemy.hp, enemy.config.maxHP);
        }
    }

    void OnDestroy()
    {
        if (enemy != null)
            enemy.OnHPChanged -= OnEnemyHPChanged;
    }

    void OnEnemyHPChanged(float hp, float maxHP)
    {
        SetHealth(hp, maxHP);
    }

    public void SetHealth(float hp, float maxHP)
    {
        target = Mathf.Clamp01(hp / maxHP);
    }

    void Update()
    {
        // 主血条平滑
        current = Mathf.Lerp(current, target, Time.deltaTime * smoothSpeed);

        if (fillMain)
            fillMain.fillAmount = current;

        // 延迟条
        if (fillDelay)
        {
            if (fillDelay.fillAmount > current)
                fillDelay.fillAmount = Mathf.MoveTowards(
                    fillDelay.fillAmount,
                    current,
                    Time.deltaTime * delaySpeed
                );
            else
                fillDelay.fillAmount = current;
        }
    }
}