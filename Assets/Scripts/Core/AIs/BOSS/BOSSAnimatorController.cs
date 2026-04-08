using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
public class BossAnimatorController : MonoBehaviour
{
    public enum AttackMotionState
    {
        None,
        AligningBeforeAttack,
        InRootMotionAttack
    }

    [Header("Refs")]
    public Transform target;
    public NavMeshAgent agent;

    [Header("Animator Params")]
    public string moveXParam = "MoveX";
    public string moveYParam = "MoveY";
    public string attackTriggerParam = "AttackTrigger";
    public string deathTriggerParam = "DeathTrigger";
    public string attackIndexParam = "AttackIndex";

    [Header("State Names")]
    public string kneelStateName = "Kneel Down";
    public string kneelIdleStateName = "Kneel Idle";
    public string standStateName = "Stand";

    [Header("Facing")]
    public bool alwaysFaceTarget = true;
    public float rotateSpeed = 12f;
    public float faceStopDistance = 0.05f;

    [Header("Attack Facing")]
    public bool alignBeforeAttack = true;
    public float preAttackFaceAngle = 10f;
    public float preAttackRotateSpeed = 18f;
    public float maxAlignTime = 0.4f;

    [Header("Animation")]
    public float moveDampTime = 0.08f;
    public float maxMoveSpeed = 3.5f;
    public bool clampInput = true;
    public bool useDeltaPositionFallback = true;

    [Header("Root Motion Attack")]
    public bool useRootMotionDuringAttack = true;
    public bool useRootRotationDuringAttack = true;

    private Animator anim;
    private Vector3 lastPosition;
    private bool isDead;

    private AttackMotionState attackMotionState = AttackMotionState.None;
    private int pendingAttackIndex = -1;
    private float alignTimer = 0f;
    private bool attackTriggered = false;

    public Animator Animator => anim;
    public bool IsAligningBeforeAttack => attackMotionState == AttackMotionState.AligningBeforeAttack;
    public bool IsInRootMotionAttack => attackMotionState == AttackMotionState.InRootMotionAttack;
    public bool IsBusyWithAttackMotion => IsAligningBeforeAttack || IsInRootMotionAttack;

    void Awake()
    {
        anim = GetComponent<Animator>();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        lastPosition = transform.position;

        if (agent != null)
        {
            agent.updateRotation = false;
            agent.updatePosition = true;
        }

        anim.applyRootMotion = false;
    }

    void Update()
    {
        if (anim == null || isDead)
            return;

        UpdateAttackMotionState();

        if (!IsBusyWithAttackMotion && target != null && ShouldFaceTargetNormally())
            FaceTargetNormally();

        UpdateMoveParams();
        lastPosition = transform.position;
    }

    void UpdateAttackMotionState()
    {
        if (attackMotionState == AttackMotionState.AligningBeforeAttack)
        {
            UpdatePreAttackAlignment();
            return;
        }

        if (attackMotionState == AttackMotionState.InRootMotionAttack)
        {
            if (!IsInAttackState())
                EndRootMotionAttack();
        }
        else
        {
            if (IsInAttackState() && attackTriggered)
                BeginRootMotionAttack();
        }
    }

    bool ShouldFaceTargetNormally()
    {
        if (!alwaysFaceTarget)
            return false;

        if (IsInDeathState())
            return false;

        if (IsInAttackState())
            return false;

        if (IsInKneelLikeState())
            return false;

        return true;
    }

    void FaceTargetNormally()
    {
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < faceStopDistance * faceStopDistance)
            return;

        Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
    }

    void UpdatePreAttackAlignment()
    {
        if (target == null)
        {
            TriggerPendingAttackImmediately();
            return;
        }

        alignTimer += Time.deltaTime;

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                preAttackRotateSpeed * Time.deltaTime
            );
        }

        float angle = GetAngleToTarget();

        if (angle <= preAttackFaceAngle || alignTimer >= maxAlignTime)
            TriggerPendingAttackImmediately();
    }

    float GetAngleToTarget()
    {
        if (target == null)
            return 0f;

        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.0001f)
            return 0f;

        return Vector3.Angle(transform.forward, toTarget.normalized);
    }

    void UpdateMoveParams()
    {
        Vector3 worldVelocity = GetWorldVelocity();
        worldVelocity.y = 0f;

        Vector3 localVelocity = transform.InverseTransformDirection(worldVelocity);

        float moveX = 0f;
        float moveY = 0f;

        if (maxMoveSpeed > 0.001f)
        {
            moveX = localVelocity.x / maxMoveSpeed;
            moveY = localVelocity.z / maxMoveSpeed;
        }

        if (clampInput)
        {
            moveX = Mathf.Clamp(moveX, -1f, 1f);
            moveY = Mathf.Clamp(moveY, -1f, 1f);
        }

        anim.SetFloat(moveXParam, moveX, moveDampTime, Time.deltaTime);
        anim.SetFloat(moveYParam, moveY, moveDampTime, Time.deltaTime);
    }

    Vector3 GetWorldVelocity()
    {
        if (agent != null && agent.enabled && attackMotionState != AttackMotionState.InRootMotionAttack)
            return agent.velocity;

        if (useDeltaPositionFallback)
        {
            Vector3 delta = transform.position - lastPosition;
            return delta / Mathf.Max(Time.deltaTime, 0.0001f);
        }

        return Vector3.zero;
    }

    public void RequestAttack(int attackIndex)
    {
        if (isDead)
            return;

        if (IsInKneelLikeState())
            return;

        pendingAttackIndex = attackIndex;
        attackTriggered = false;

        if (alignBeforeAttack && target != null)
        {
            attackMotionState = AttackMotionState.AligningBeforeAttack;
            alignTimer = 0f;

            if (agent != null && agent.enabled)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
        }
        else
        {
            TriggerPendingAttackImmediately();
        }
    }

    void TriggerPendingAttackImmediately()
    {
        if (pendingAttackIndex < 0 || isDead)
            return;

        if (IsInKneelLikeState())
        {
            pendingAttackIndex = -1;
            attackTriggered = false;
            attackMotionState = AttackMotionState.None;
            return;
        }

        attackMotionState = AttackMotionState.None;
        alignTimer = 0f;

        anim.SetInteger(attackIndexParam, pendingAttackIndex);
        anim.SetTrigger(attackTriggerParam);

        attackTriggered = true;
        pendingAttackIndex = -1;
    }

    void BeginRootMotionAttack()
    {
        attackMotionState = AttackMotionState.InRootMotionAttack;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.updatePosition = false;
            agent.updateRotation = false;
        }

        anim.applyRootMotion = useRootMotionDuringAttack;
    }

    void EndRootMotionAttack()
    {
        attackMotionState = AttackMotionState.None;
        attackTriggered = false;
        pendingAttackIndex = -1;
        alignTimer = 0f;

        anim.applyRootMotion = false;

        if (agent != null && agent.enabled)
        {
            agent.updatePosition = true;
            agent.updateRotation = false;
            agent.Warp(transform.position);
            agent.isStopped = false;
        }
    }

    void OnAnimatorMove()
    {
        if (anim == null)
            return;

        if (IsInAttackState() && attackTriggered && attackMotionState != AttackMotionState.InRootMotionAttack)
            BeginRootMotionAttack();

        if (attackMotionState != AttackMotionState.InRootMotionAttack)
            return;

        Vector3 deltaPos = anim.deltaPosition;
        Quaternion deltaRot = anim.deltaRotation;

        if (useRootMotionDuringAttack)
            transform.position += deltaPos;

        if (useRootRotationDuringAttack)
            transform.rotation = transform.rotation * deltaRot;

        if (agent != null && agent.enabled)
            agent.nextPosition = transform.position;
    }

    public void PlayDeath()
    {
        if (isDead)
            return;

        isDead = true;
        attackMotionState = AttackMotionState.None;
        pendingAttackIndex = -1;
        attackTriggered = false;

        anim.applyRootMotion = false;
        anim.SetTrigger(deathTriggerParam);

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.updatePosition = false;
            agent.updateRotation = false;
        }
    }

    public bool IsInAttackState()
    {
        if (anim == null) return false;

        AnimatorStateInfo current = anim.GetCurrentAnimatorStateInfo(0);

        if (anim.IsInTransition(0))
        {
            AnimatorStateInfo next = anim.GetNextAnimatorStateInfo(0);
            return current.IsTag("Attack") || next.IsTag("Attack");
        }

        return current.IsTag("Attack");
    }

    public bool IsInDeathState()
    {
        if (anim == null) return false;

        AnimatorStateInfo current = anim.GetCurrentAnimatorStateInfo(0);

        if (anim.IsInTransition(0))
        {
            AnimatorStateInfo next = anim.GetNextAnimatorStateInfo(0);
            return current.IsTag("Death") || next.IsTag("Death");
        }

        return current.IsTag("Death");
    }

    public bool IsInStateByName(string stateName)
    {
        if (anim == null || string.IsNullOrEmpty(stateName))
            return false;

        AnimatorStateInfo current = anim.GetCurrentAnimatorStateInfo(0);
        if (current.IsName(stateName))
            return true;

        if (anim.IsInTransition(0))
        {
            AnimatorStateInfo next = anim.GetNextAnimatorStateInfo(0);
            if (next.IsName(stateName))
                return true;
        }

        return false;
    }

    public bool IsInKneelState()
    {
        return IsInStateByName(kneelStateName);
    }

    public bool IsInKneelIdleState()
    {
        return IsInStateByName(kneelIdleStateName);
    }

    public bool IsInStandState()
    {
        return IsInStateByName(standStateName);
    }

    public bool IsInKneelLikeState()
    {
        return IsInKneelState() || IsInKneelIdleState() || IsInStandState();
    }

    public float GetCurrentNormalizedTime()
    {
        if (anim == null)
            return 0f;

        return anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}