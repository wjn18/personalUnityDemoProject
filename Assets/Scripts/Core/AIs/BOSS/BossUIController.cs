using UnityEngine;
using UnityEngine.UI;

public class BossUIController : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;
    public BOSSAI bossAI;
    public BossStaggerSystem stagger;

    [Header("HP UI")]
    public Image hpFillMain;
    public Image hpFillDelay;

    [Header("RV UI")]
    public Image rvFillMain;
    public Image rvFillDelay;

    [Header("Show Range")]
    public float showDistance = 50f;

    [Header("Fill Smooth")]
    public float mainSmoothSpeed = 12f;
    public float delayDropSpeed = 1.5f;

    [Header("Fade")]
    public CanvasGroup canvasGroup;
    public float fadeSpeed = 2f;

    [Header("Defeated Text")]
    public GameObject defeatedText;

    float hpMainValue = 1f;
    float hpDelayValue = 1f;
    float rvMainValue = 1f;
    float rvDelayValue = 1f;

    bool isDead = false;
    bool isFadingOut = false;

    void Start()
    {
        if (bossAI != null && bossAI.maxHP > 0f)
        {
            float hpPercent = Mathf.Clamp01(bossAI.currentHP / bossAI.maxHP);
            hpMainValue = hpPercent;
            hpDelayValue = hpPercent;
        }

        if (stagger != null)
        {
            float rvPercent = Mathf.Clamp01(stagger.GetRVPercent());
            rvMainValue = rvPercent;
            rvDelayValue = rvPercent;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        if (defeatedText != null)
            defeatedText.SetActive(false);

        ApplyFillAmounts();
    }

    void Update()
    {
        if (player == null || bossAI == null || stagger == null)
            return;

        float dist = Vector3.Distance(player.position, bossAI.transform.position);
        bool inRange = dist <= showDistance;

        // 第一次检测到Boss死亡
        if (!isDead && bossAI.currentHP <= 0f)
        {
            isDead = true;
            isFadingOut = true;

            if (defeatedText != null)
                defeatedText.SetActive(true);
        }

        // 死亡后：先淡出，淡出完就彻底隐藏并停止后续显示逻辑
        if (isDead)
        {
            if (isFadingOut)
            {
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, 0f, fadeSpeed * Time.deltaTime);

                    if (canvasGroup.alpha <= 0.001f)
                    {
                        canvasGroup.alpha = 0f;
                        isFadingOut = false;
                        gameObject.SetActive(false); // 彻底隐藏
                    }
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }

            return; // 关键：死亡后不再执行下面的显示逻辑
        }

        // 没死亡时，根据距离显示/隐藏
        if (canvasGroup != null)
            canvasGroup.alpha = inRange ? 1f : 0f;

        if (!inRange)
            return;

        float targetHP = 0f;
        if (bossAI.maxHP > 0f)
            targetHP = Mathf.Clamp01(bossAI.currentHP / bossAI.maxHP);

        float targetRV = Mathf.Clamp01(stagger.GetRVPercent());

        hpMainValue = Mathf.MoveTowards(hpMainValue, targetHP, mainSmoothSpeed * Time.deltaTime);
        rvMainValue = Mathf.MoveTowards(rvMainValue, targetRV, mainSmoothSpeed * Time.deltaTime);

        if (hpDelayValue > hpMainValue)
            hpDelayValue = Mathf.MoveTowards(hpDelayValue, hpMainValue, delayDropSpeed * Time.deltaTime);
        else
            hpDelayValue = hpMainValue;

        if (rvDelayValue > rvMainValue)
            rvDelayValue = Mathf.MoveTowards(rvDelayValue, rvMainValue, delayDropSpeed * Time.deltaTime);
        else
            rvDelayValue = rvMainValue;

        ApplyFillAmounts();
    }

    void ApplyFillAmounts()
    {
        if (hpFillMain != null) hpFillMain.fillAmount = hpMainValue;
        if (hpFillDelay != null) hpFillDelay.fillAmount = hpDelayValue;

        if (rvFillMain != null) rvFillMain.fillAmount = rvMainValue;
        if (rvFillDelay != null) rvFillDelay.fillAmount = rvDelayValue;
    }
}