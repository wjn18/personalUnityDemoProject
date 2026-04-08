using UnityEngine;

public class BossStaggerSystem : MonoBehaviour
{
    enum KneelSequencePhase
    {
        None,
        WaitingKneelState,
        WaitingKneelIdleState,
        HoldingKneelIdle,
        WaitingStandState,
        PlayingStand
    }

    [Header("Refs")]
    public BOSSAI bossAI;
    public BossAnimatorController animatorController;

    [Header("Animator Params")]
    public string hitTriggerParam = "HitTrigger";
    public string kneelTriggerParam = "KneelTrigger";

    [Header("RV")]
    public float maxRV = 200f;
    public float currentRV = 200f;
    [Range(0f, 1f)]
    public float alwaysOpenThresholdPercent = 0.3f;

    [Header("Recovery")]
    public float recoverDelay = 3f;
    public float recoverPerSecond = 10f;

    [Header("Stagger Window Cycle")]
    public float initialStaggerWindowDuration = 2f;
    public float superArmorDuration = 6f;

    [Header("Kneel Idle Hold")]
    public float kneelIdleHoldDuration = 2f;

    [Header("Stand Bool Release")]
    public float standBoolReleaseDelay = 1f;

    [Header("Execution")]
    public float executeDistance = 4f;
    public float executeDamage = 80f;

    float lastHitTime = -999f;

    bool staggerWindowOpen = true;
    float staggerCycleTimer = 0f;

    KneelSequencePhase kneelPhase = KneelSequencePhase.None;
    float kneelIdleHoldTimer = 0f;

    bool waitingToReleaseKneelBool = false;
    float releaseKneelBoolTimer = 0f;

    void Awake()
    {
        if (bossAI == null)
            bossAI = GetComponent<BOSSAI>();

        if (animatorController == null)
            animatorController = GetComponent<BossAnimatorController>();

        currentRV = Mathf.Clamp(currentRV, 0f, maxRV);
        staggerWindowOpen = true;
        staggerCycleTimer = 0f;
    }

    void Update()
    {
        UpdateRecovery();
        UpdateStaggerWindowCycle();
        UpdateKneelSequence();
        UpdateKneelBoolReleaseTimer();
    }

    void UpdateRecovery()
    {
        if (IsInKneelSequence())
            return;

        if (currentRV >= maxRV)
            return;

        if (Time.time - lastHitTime < recoverDelay)
            return;

        currentRV += recoverPerSecond * Time.deltaTime;
        currentRV = Mathf.Clamp(currentRV, 0f, maxRV);
    }

    void UpdateStaggerWindowCycle()
    {
        if (IsInKneelSequence())
            return;

        if (currentRV <= maxRV * alwaysOpenThresholdPercent)
        {
            staggerWindowOpen = true;
            staggerCycleTimer = 0f;
            return;
        }

        staggerCycleTimer += Time.deltaTime;

        if (staggerWindowOpen)
        {
            if (staggerCycleTimer >= initialStaggerWindowDuration)
            {
                staggerWindowOpen = false;
                staggerCycleTimer = 0f;
            }
        }
        else
        {
            if (staggerCycleTimer >= superArmorDuration)
            {
                staggerWindowOpen = true;
                staggerCycleTimer = 0f;
            }
        }
    }

    void UpdateKneelSequence()
    {
        if (animatorController == null || animatorController.Animator == null)
            return;

        switch (kneelPhase)
        {
            case KneelSequencePhase.None:
                return;

            case KneelSequencePhase.WaitingKneelState:
                if (animatorController.IsInKneelState())
                    kneelPhase = KneelSequencePhase.WaitingKneelIdleState;
                return;

            case KneelSequencePhase.WaitingKneelIdleState:
                if (animatorController.IsInKneelIdleState())
                {
                    kneelPhase = KneelSequencePhase.HoldingKneelIdle;
                    kneelIdleHoldTimer = 0f;
                }
                return;

            case KneelSequencePhase.HoldingKneelIdle:
                kneelIdleHoldTimer += Time.deltaTime;
                if (kneelIdleHoldTimer >= kneelIdleHoldDuration)
                {
                    TriggerStand();
                }
                return;

            case KneelSequencePhase.WaitingStandState:
                if (animatorController.IsInStandState())
                {
                    kneelPhase = KneelSequencePhase.PlayingStand;
                    waitingToReleaseKneelBool = true;
                    releaseKneelBoolTimer = 0f;
                }
                return;

            case KneelSequencePhase.PlayingStand:
                if (!animatorController.IsInStandState())
                {
                    FinishKneelSequence();
                }
                return;
        }
    }

    void UpdateKneelBoolReleaseTimer()
    {
        if (!waitingToReleaseKneelBool)
            return;

        releaseKneelBoolTimer += Time.deltaTime;
        if (releaseKneelBoolTimer >= standBoolReleaseDelay)
        {
            waitingToReleaseKneelBool = false;
            releaseKneelBoolTimer = 0f;
            
        }
    }

    public void TakeStaggerDamage(float amount)
    {
        if (bossAI == null || animatorController == null || animatorController.Animator == null)
            return;

        if (bossAI.currentState == BOSSAI.BossState.Dead)
            return;

        if (IsInKneelSequence())
            return;

        lastHitTime = Time.time;
        currentRV = Mathf.Clamp(currentRV - Mathf.Max(0f, amount), 0f, maxRV);

        if (currentRV <= 0f)
        {
            EnterKneel();
            return;
        }

        if (!CanPlayHitReaction())
            return;

        animatorController.Animator.SetTrigger(hitTriggerParam);
    }

    bool CanPlayHitReaction()
    {
        if (bossAI == null || animatorController == null)
            return false;

        if (IsInKneelSequence())
            return false;

        if (animatorController.IsInKneelLikeState())
            return false;

        if (bossAI.currentState == BOSSAI.BossState.Attacking || animatorController.IsInAttackState())
            return false;

        if (currentRV <= maxRV * alwaysOpenThresholdPercent)
            return true;

        return staggerWindowOpen;
    }

    void EnterKneel()
    {
        if (animatorController == null || animatorController.Animator == null)
            return;

        kneelPhase = KneelSequencePhase.WaitingKneelState;
        kneelIdleHoldTimer = 0f;

        waitingToReleaseKneelBool = false;
        releaseKneelBoolTimer = 0f;


        if (bossAI != null)
        {
            bossAI.currentState = BOSSAI.BossState.Idle;
            bossAI.SendMessage("StopMove", SendMessageOptions.DontRequireReceiver);
        }

        animatorController.Animator.ResetTrigger(hitTriggerParam);
        animatorController.Animator.SetTrigger(kneelTriggerParam);
    }

    void TriggerStand()
    {
        if (animatorController == null || animatorController.Animator == null)
            return;

        kneelPhase = KneelSequencePhase.WaitingStandState;
    }

    void FinishKneelSequence()
    {
        kneelPhase = KneelSequencePhase.None;
        kneelIdleHoldTimer = 0f;

        waitingToReleaseKneelBool = false;
        releaseKneelBoolTimer = 0f;

        currentRV = maxRV;
        lastHitTime = Time.time;
    }

    public bool TryExecute(Transform executor)
    {
        if (!IsInKneelSequence() || bossAI == null || animatorController == null || animatorController.Animator == null)
            return false;

        if (executor == null)
            return false;

        float dist = Vector3.Distance(executor.position, transform.position);
        if (dist > executeDistance)
            return false;

        bossAI.TakeDamage(executeDamage, executor.gameObject);

        animatorController.Animator.ResetTrigger(kneelTriggerParam);
        animatorController.Animator.SetTrigger(hitTriggerParam);

        FinishKneelSequence();

        return true;
    }

    public bool ShouldLockBossAction()
    {
        if (bossAI != null && bossAI.currentState == BOSSAI.BossState.Dead)
            return true;

        if (IsInKneelSequence())
            return true;

        if (animatorController != null && animatorController.IsInKneelLikeState())
            return true;

        return false;
    }

    public bool IsKneelingOrStanding()
    {
        if (IsInKneelSequence())
            return true;

        if (animatorController != null && animatorController.IsInKneelLikeState())
            return true;

        return false;
    }

    bool IsInKneelSequence()
    {
        return kneelPhase != KneelSequencePhase.None;
    }

    public float GetRVPercent()
    {
        if (maxRV <= 0f)
            return 0f;

        return Mathf.Clamp01(currentRV / maxRV);
    }


}