using UnityEngine;

public partial class PlayerController
{
    bool IsMoveHeld()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");
        Vector2 input = new Vector2(inputX, inputY);
        return input.sqrMagnitude > (moveCancelInputThreshold * moveCancelInputThreshold);
    }

    bool HasAnimatorParameter(string paramName)
    {
        if (animator == null || string.IsNullOrEmpty(paramName)) return false;

        foreach (var p in animator.parameters)
        {
            if (p.name == paramName) return true;
        }
        return false;
    }

    void UpdateBlockReentryLock()
    {
        if (!blockLockedByLowSP)
            return;

        float minSP = GetBlockReenterMinSP();
        if (stats == null || stats.sp >= minSP)
            blockLockedByLowSP = false;
    }

    public float GetBlockReenterMinSP()
    {
        if (stats == null)
            return Mathf.Max(0f, blockSPCostPerHit);

        return Mathf.Max(0f, stats.maxSP * Mathf.Clamp01(blockReenterSPPercent));
    }

    public bool IsBlockLockedBySPRecovery()
    {
        return blockLockedByLowSP;
    }

    void ClearAttackMoveSpeedOverrideInternal()
    {
        useAttackMoveSpeedOverride = false;
        attackMoveSpeedOverride = 0f;
    }

    void ClearRollMoveSpeedOverrideInternal()
    {
        useRollMoveSpeedOverride = false;
        rollMoveSpeedOverride = 0f;
    }

    void EndRollState()
    {
        isRolling = false;
        ClearRollMoveSpeedOverrideInternal();
    }

    void EndAttackState()
    {
        isAttacking = false;
        sprintAttackActive = false;
        heavyAttackActive = false;
        queueNextAttack = false;
        currentAttackStep = 0;
        canMoveCancelAttack = false;
        moveWasHeldWhenCancelWindowOpened = false;
        attackWindowActive = false;
        ClearAttackMoveSpeedOverrideInternal();
        ClearCurrentAttackData();
        hitTargetsThisSwing.Clear();
        lastAttackFinishedTime = Time.time;

        animator.SetBool(queueNextAttackParam, false);
    }

    void ClearAttackStateForActionStart()
    {
        isAttacking = false;
        sprintAttackActive = false;
        heavyAttackActive = false;
        queueNextAttack = false;
        currentAttackStep = 0;
        canMoveCancelAttack = false;
        moveWasHeldWhenCancelWindowOpened = false;
        attackWindowActive = false;
        ClearAttackMoveSpeedOverrideInternal();
        ClearCurrentAttackData();
        hitTargetsThisSwing.Clear();
        animator.SetBool(queueNextAttackParam, false);
    }

    void ClearActionStateForHit(bool keepBlocking, bool keepSprint)
    {
        EndRollState();
        isAttacking = false;
        sprintAttackActive = false;
        heavyAttackActive = false;
        queueNextAttack = false;
        currentAttackStep = 0;
        canMoveCancelAttack = false;
        moveWasHeldWhenCancelWindowOpened = false;
        attackWindowActive = false;
        ClearAttackMoveSpeedOverrideInternal();
        ClearCurrentAttackData();
        hitTargetsThisSwing.Clear();
        animator.SetBool(queueNextAttackParam, false);

        if (!keepSprint)
            sprintMode = false;

        if (!keepBlocking)
            StopBlockingInternal();
    }

    PlayerAttackData GetAttackDataForStep(int step)
    {
        switch (step)
        {
            case 1: return attack1Data;
            case 2: return attack2Data;
            case 3: return attack3Data;
            case 4: return attack4Data;
            default: return null;
        }
    }

    void SetCurrentAttackData(PlayerAttackData data)
    {
        currentAttackData = data;
    }

    void ClearCurrentAttackData()
    {
        currentAttackData = null;
    }

    float GetCurrentAttackDamage()
    {
        float baseDamage = fallbackAttackDamage;

        if (currentAttackData != null)
            baseDamage = currentAttackData.damage;

        if (isBerserkActive)
            baseDamage *= berserkDamageMultiplier;

        return Mathf.Max(0f, baseDamage);
    }

    float GetCurrentAPGainPerHit()
    {
        float baseGain = 0f;

        if (currentAttackData != null)
            baseGain = currentAttackData.apGainPerHit;

        if (isBerserkActive)
            baseGain *= berserkAPGainMultiplier;

        return Mathf.Max(0f, baseGain);
    }

    float GetCurrentAttackRadius()
    {
        float radius = Mathf.Max(0f, attackRadius);

        if (isBerserkActive)
            radius *= berserkAttackRadiusMultiplier;

        return radius;
    }

    bool HasEnoughSPForAction(PlayerAttackData data)
    {
        if (stats == null || data == null)
            return true;

        return stats.HasEnoughSP(data.spCost);
    }

    bool TrySpendSPForAction(PlayerAttackData data)
    {
        if (stats == null || data == null)
            return true;

        return stats.SpendSP(data.spCost);
    }

    bool TrySpendSP(float amount)
    {
        if (stats == null)
            return true;

        return stats.SpendSP(amount);
    }

    public float GetIncomingDamageMultiplier()
    {
        if (isBerserkActive)
            return 1f;

        if (!isBlocking || isDead || IsBlockLockedBySPRecovery())
            return 1f;

        if (!CanBlockCurrentHit())
            return 1f;

        return Mathf.Clamp01(blockDamageMultiplier);
    }

    public float ModifyIncomingDamage(float incomingDamage)
    {
        return Mathf.Max(0f, incomingDamage) * GetIncomingDamageMultiplier();
    }

    public float GetBlockSPCostPerHit()
    {
        return Mathf.Max(0f, blockSPCostPerHit);
    }
}