using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUDController : MonoBehaviour
{
    [Header("Bind")]
    public PlayerStatsRuntime stats;

    [Header("UI")]
    public Slider hpSlider;
    public Slider spSlider;
    public Slider apSlider;
    public Slider expSlider;

    public TextMeshProUGUI hpText;
    public TextMeshProUGUI spText;
    public TextMeshProUGUI apText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI expText;

    void Awake()
    {
        if (stats == null)
            stats = FindFirstObjectByType<PlayerStatsRuntime>();
    }

    void OnEnable()
    {
        if (stats == null) return;

        stats.OnHPChanged += HandleHP;
        stats.OnSPChanged += HandleSP;
        stats.OnAPChanged += HandleAP;
        stats.OnExpChanged += HandleExp;
        stats.OnLevelChanged += HandleLevel;

        RefreshUI();
    }

    void OnDisable()
    {
        if (stats == null) return;

        stats.OnHPChanged -= HandleHP;
        stats.OnSPChanged -= HandleSP;
        stats.OnAPChanged -= HandleAP;
        stats.OnExpChanged -= HandleExp;
        stats.OnLevelChanged -= HandleLevel;
    }

    void RefreshUI()
    {
        HandleHP(stats.hp, stats.maxHP);
        HandleSP(stats.sp, stats.maxSP);
        HandleAP(stats.ap, stats.maxAP);
        HandleExp(stats.exp, stats.expToNext);
        HandleLevel(stats.level);
    }

    void HandleHP(float hp, float maxHP)
    {
        if (hpSlider != null)
            hpSlider.value = (maxHP <= 0f) ? 0f : hp / maxHP;

        if (hpText != null)
            hpText.text = $"{Mathf.CeilToInt(hp)}/{Mathf.CeilToInt(maxHP)}";
    }

    void HandleSP(float sp, float maxSP)
    {
        if (spSlider != null)
            spSlider.value = (maxSP <= 0f) ? 0f : sp / maxSP;

        if (spText != null)
            spText.text = $"{Mathf.CeilToInt(sp)}/{Mathf.CeilToInt(maxSP)}";
    }

    void HandleAP(float ap, float maxAP)
    {
        if (apSlider != null)
            apSlider.value = (maxAP <= 0f) ? 0f : ap / maxAP;

        if (apText != null)
            apText.text = $"{Mathf.CeilToInt(ap)}/{Mathf.CeilToInt(maxAP)}";
    }

    void HandleExp(int exp, int expToNext)
    {
        if (expSlider != null)
            expSlider.value = (expToNext <= 0) ? 0f : (float)exp / expToNext;

        if (expText != null)
            expText.text = $"{exp}/{expToNext}";
    }

    void HandleLevel(int level)
    {
        if (levelText != null)
            levelText.text = $"Lv {level}";
    }
}