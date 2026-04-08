using UnityEngine;

public partial class PlayerController
{
    void HandleMovement()
    {
        bool lockedOn = IsLockedOn();
        bool inCombatAnimation = IsInCombatLikeAnimation();
        bool inHitReactionAnimation = IsInHitReactionAnimation();
        bool inAttackAnimation = IsInAttackAnimation();
        bool inPowerUpAnimation = IsInPowerUpAnimation();

        // 不只依赖 Animator tag，直接用运行时状态锁动作输入
        bool shouldForceActionMovement =
            isAttacking ||
            sprintAttackActive ||
            heavyAttackActive ||
            isRolling ||
            isInHitReaction ||
            isPoweringUp ||
            inAttackAnimation ||
            inHitReactionAnimation ||
            inPowerUpAnimation;

        bool lockPlayerMoveInput =
            disableWASDMovementDuringCombatAnimations && shouldForceActionMovement;

        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        Vector3 move = Vector3.zero;

        if (lockedOn)
        {
            Transform target = playerLockOn.GetTargetTransform();

            bool allowLockOnFacing =
                target != null &&
                !isRolling &&
                !isAttacking &&
                !sprintAttackActive &&
                !heavyAttackActive &&
                !inHitReactionAnimation &&
                !inAttackAnimation &&
                !inPowerUpAnimation &&
                !isPoweringUp;

            if (allowLockOnFacing)
            {
                float faceSpeed = lockRotationSpeed;

                if (isBlocking)
                {
                    if (autoFaceLockTargetWhileBlocking)
                        faceSpeed = Mathf.Max(lockRotationSpeed, blockLockRotationSpeed);
                    else
                        target = null;
                }

                if (target != null)
                    FaceTarget(target, faceSpeed);
            }
        }

        if (lockPlayerMoveInput)
        {
            move = GetActionAnimationMove();

            currentMoveX = Mathf.SmoothDamp(currentMoveX, 0f, ref moveXVelocity, moveParamSmoothTime);
            currentMoveY = Mathf.SmoothDamp(currentMoveY, 0f, ref moveYVelocity, moveParamSmoothTime);
        }
        else if (lockedOn)
        {
            Vector3 worldMove = GetNormalizedWorldInput(inputX, inputY);

            if (worldMove.sqrMagnitude > 0.0001f)
                lastMoveInput = worldMove;

            float animMoveX = Vector3.Dot(worldMove, transform.right);
            float animMoveY = Vector3.Dot(worldMove, transform.forward);

            float speedMul = (animMoveY < -0.01f) ? backwardSpeedMultiplier : 1f;
            float baseSpeed = (sprintMode ? sprintMoveSpeed : lockMoveSpeed) * speedMul;

            if (inCombatAnimation)
                baseSpeed = GetCombatMoveSpeed(baseSpeed);

            move = worldMove * baseSpeed;

            currentMoveX = Mathf.SmoothDamp(currentMoveX, animMoveX, ref moveXVelocity, moveParamSmoothTime);
            currentMoveY = Mathf.SmoothDamp(currentMoveY, animMoveY, ref moveYVelocity, moveParamSmoothTime);
        }
        else
        {
            Vector3 freeInput = GetNormalizedWorldInput(inputX, inputY);

            if (freeInput.sqrMagnitude > 0.0001f)
                lastMoveInput = freeInput;

            float baseSpeed = sprintMode ? sprintMoveSpeed : freeMoveSpeed;

            if (inCombatAnimation)
                baseSpeed = GetCombatMoveSpeed(baseSpeed);

            move = freeInput * baseSpeed;

            bool canRotate =
                !inCombatAnimation &&
                !inPowerUpAnimation &&
                !isAttacking &&
                !sprintAttackActive &&
                !heavyAttackActive &&
                freeInput.sqrMagnitude > 0.0001f;

            if (canRotate)
            {
                Quaternion targetRot = Quaternion.LookRotation(freeInput, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    freeRotationSpeed * Time.deltaTime
                );
            }

            currentMoveX = Mathf.SmoothDamp(currentMoveX, 0f, ref moveXVelocity, moveParamSmoothTime);
            currentMoveY = Mathf.SmoothDamp(currentMoveY, 0f, ref moveYVelocity, moveParamSmoothTime);
        }

        currentSpeed = new Vector3(move.x, 0f, move.z).magnitude;

        ApplyGravity(ref move);
        characterController.Move(move * Time.deltaTime);
    }

    Vector3 GetActionAnimationMove()
    {
        if (IsInHitReactionAnimation() || isInHitReaction)
            return Vector3.zero;

        if (IsInPowerUpAnimation() || isPoweringUp)
            return Vector3.zero;

        // 只要运行时处于攻击状态，就强制使用缓存方向
        if (isAttacking || sprintAttackActive || heavyAttackActive || IsInAttackAnimation())
        {
            float attackSpeed = useAttackMoveSpeedOverride ? attackMoveSpeedOverride : 0f;
            return cachedAttackMotionDirection * attackSpeed;
        }

        if (isRolling)
        {
            if (useStoredRollMotion || allowRollMovementAssist)
            {
                float finalRollSpeed = useRollMoveSpeedOverride
                    ? rollMoveSpeedOverride
                    : rollMoveSpeed;

                return cachedRollMotionDirection * finalRollSpeed;
            }

            return Vector3.zero;
        }

        return Vector3.zero;
    }

    Vector3 GetNormalizedWorldInput(float inputX, float inputY)
    {
        Vector3 input = new Vector3(inputX, 0f, inputY);
        if (input.sqrMagnitude > 1f)
            input.Normalize();

        return input;
    }

    Vector3 GetSafeHorizontalDirection(Vector3 dir, Vector3 fallback)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
            return dir.normalized;

        fallback.y = 0f;
        if (fallback.sqrMagnitude > 0.0001f)
            return fallback.normalized;

        return Vector3.forward;
    }

    Vector3 GetRollStartDirection()
    {
        Vector3 inputDir = GetNormalizedWorldInput(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        if (inputDir.sqrMagnitude > 0.0001f)
            return inputDir;

        return GetSafeHorizontalDirection(lastMoveInput, transform.forward);
    }

    void CacheStandardAttackMotionDirection()
    {
        cachedAttackMotionDirection = GetSafeHorizontalDirection(transform.forward, lastMoveInput);
    }

    void CacheSprintAttackMotionDirection()
    {
        Vector3 inputDir = GetNormalizedWorldInput(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        if (inputDir.sqrMagnitude > 0.0001f)
        {
            cachedAttackMotionDirection = inputDir;
            lastMoveInput = inputDir;
            return;
        }

        cachedAttackMotionDirection = GetSafeHorizontalDirection(lastMoveInput, transform.forward);
    }

    void CorrectFacingBeforeLockOnAttack()
    {
        if (!snapToLockTargetBeforeAttack)
            return;

        if (!IsLockedOn())
            return;

        if (playerLockOn == null)
            return;

        Transform target = playerLockOn.GetTargetTransform();
        if (target == null)
            return;

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.0001f)
            return;

        Vector3 currentForward = transform.forward;
        currentForward.y = 0f;

        if (currentForward.sqrMagnitude < 0.0001f)
            currentForward = Vector3.forward;

        float angle = Vector3.Angle(currentForward.normalized, toTarget.normalized);
        if (angle > lockAttackFacingMaxAngle)
            return;

        transform.rotation = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
    }

    void HandleSPRecovery()
    {
        if (stats == null)
            return;

        bool allowRecovery = true;

        if (blockStopsSPRecovery && isBlocking)
            allowRecovery = false;

        if (sprintStopsSPRecovery && sprintMode)
            allowRecovery = false;

        float recoveryDelta = Time.deltaTime * GetCurrentSPRecoveryMultiplier();
        stats.UpdateSPRecovery(allowRecovery, recoveryDelta);
    }

    float GetCurrentSPRecoveryMultiplier()
    {
        return isBerserkActive ? Mathf.Max(1f, berserkSPRecoveryMultiplier) : 1f;
    }

    float GetCombatMoveSpeed(float baseSpeed)
    {
        if ((isAttacking || sprintAttackActive || heavyAttackActive || IsInAttackAnimation()) && useAttackMoveSpeedOverride)
            return attackMoveSpeedOverride;

        return baseSpeed * combatMoveMultiplier;
    }

    void FaceTarget(Transform target, float rotSpeed)
    {
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            rotSpeed * Time.deltaTime
        );
    }

    void ApplyGravity(ref Vector3 move)
    {
        if (characterController.isGrounded)
            verticalVelocity = groundedStickForce;
        else
            verticalVelocity += gravity * Time.deltaTime;

        move.y = verticalVelocity;
    }

    void ApplyGravityOnly()
    {
        Vector3 move = Vector3.zero;
        ApplyGravity(ref move);
        characterController.Move(move * Time.deltaTime);
    }
}