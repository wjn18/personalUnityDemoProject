using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(BossAnimatorController))]
public class BOSSAI : MonoBehaviour, IDamageable
{
    public enum BossState
    {
        Idle,
        Approach,
        Retreat,
        Attacking,
        Dead
    }

    [Header("Refs")]
    public Transform player;
    public NavMeshAgent agent;
    public BossAnimatorController animatorController;
    public BossMeleeDamageWindow meleeDamageWindow;
    public BossRangedSkillCaster rangedSkillCaster;
    public BossStaggerSystem staggerSystem;
    public BossWeaponTrailController weaponTrailVFX;
    public CombatAudioController combatAudioController;

    [Header("Stats")]
    public float maxHP = 1000f;
    public float currentHP = 1000f;

    [Header("Detection")]
    public float engageDistance = 25f;

    [Header("Locomotion")]
    public float approachStopDistance = 3.5f;
    public float repathInterval = 0.15f;
    public float combatDistanceTolerance = 0.35f;

    [Header("Attacks")]
    public BossAttackDefinition[] attacks = CreateDefaultAttackSet();

    [Header("Runtime")]
    public BossState currentState = BossState.Idle;
    public int currentAttackIndex = -1;
    public BossAttackPhase currentAttackPhase = BossAttackPhase.None;

    [Header("Debug")]
    public bool drawGizmos = true;

    private readonly Dictionary<int, float> nextReadyTimeByIndex = new Dictionary<int, float>();

    private BossAttackDefinition currentAttack;
    private float currentAttackElapsed;
    private float lastRepathTime;
    private int currentAttackWindowIndex;
    private bool attackHitConfirmedThisWindow;
    private bool rangedAttackResultPending;

    void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animatorController == null)
            animatorController = GetComponent<BossAnimatorController>();

        if (staggerSystem == null)
            staggerSystem = GetComponent<BossStaggerSystem>();

        if (weaponTrailVFX == null)
            weaponTrailVFX = GetComponentInChildren<BossWeaponTrailController>(true);

        if (combatAudioController == null)
            combatAudioController = GetComponentInChildren<CombatAudioController>(true);

        agent.updateRotation = false;
        currentHP = Mathf.Clamp(currentHP, 0f, maxHP);

        if (attacks == null || attacks.Length == 0)
            attacks = CreateDefaultAttackSet();

        BuildCooldownTable();
    }

    void Start()
    {
        if (animatorController != null && player != null)
            animatorController.SetTarget(player);

        if (meleeDamageWindow != null)
            meleeDamageWindow.ownerAI = this;

        if (rangedSkillCaster != null)
            rangedSkillCaster.ownerAI = this;
    }

    void Update()
    {
        if (player == null || currentState == BossState.Dead)
            return;

        if (staggerSystem != null && staggerSystem.ShouldLockBossAction())
        {
            ForceInterruptAction();
            return;
        }

        if (currentHP <= 0f)
        {
            Die();
            return;
        }

        if (currentState == BossState.Attacking)
        {
            UpdateAttackState();
            return;
        }

        float distance = DistanceToPlayer();
        if (distance > engageDistance)
        {
            StopMove();
            currentState = BossState.Idle;
            return;
        }

        BossAttackDefinition attack = SelectAttack(distance);
        if (attack != null)
        {
            StartAttack(attack);
            return;
        }

        UpdatePositioning(distance);
    }

    void BuildCooldownTable()
    {
        nextReadyTimeByIndex.Clear();

        if (attacks == null)
            return;

        for (int i = 0; i < attacks.Length; i++)
        {
            BossAttackDefinition attack = attacks[i];
            if (attack == null)
                continue;

            if (!nextReadyTimeByIndex.ContainsKey(attack.attackIndex))
                nextReadyTimeByIndex.Add(attack.attackIndex, 0f);
        }
    }

    float DistanceToPlayer()
    {
        Vector3 a = transform.position;
        Vector3 b = player.position;
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    BossAttackDefinition SelectAttack(float distanceToPlayer)
    {
        if (attacks == null || attacks.Length == 0)
            return null;

        List<BossAttackDefinition> candidates = new List<BossAttackDefinition>();
        int bestScore = int.MinValue;

        for (int i = 0; i < attacks.Length; i++)
        {
            BossAttackDefinition attack = attacks[i];
            if (attack == null)
                continue;

            if (!IsAttackReady(attack))
                continue;

            if (!attack.IsInRange(distanceToPlayer))
                continue;

            int score = EvaluateAttackScore(attack, distanceToPlayer);
            if (score > bestScore)
            {
                bestScore = score;
                candidates.Clear();
                candidates.Add(attack);
            }
            else if (score == bestScore)
            {
                candidates.Add(attack);
            }
        }

        if (candidates.Count == 0)
            return null;

        return candidates[Random.Range(0, candidates.Count)];
    }

    int EvaluateAttackScore(BossAttackDefinition attack, float distanceToPlayer)
    {
        int score = attack.priority * 1000;
        score -= Mathf.RoundToInt(Mathf.Abs(attack.PreferredRange - distanceToPlayer) * 100f);

        switch (attack.category)
        {
            case BossAttackCategory.MeleeSkill:
                score += 100;
                break;
            case BossAttackCategory.Ranged:
                score += 50;
                break;
        }

        return score;
    }

    bool IsAttackReady(BossAttackDefinition attack)
    {
        if (attack == null)
            return false;

        if (!nextReadyTimeByIndex.TryGetValue(attack.attackIndex, out float readyTime))
            return true;

        return Time.time >= readyTime;
    }

    void SetAttackCooldown(BossAttackDefinition attack)
    {
        if (attack == null)
            return;

        nextReadyTimeByIndex[attack.attackIndex] = Time.time + Mathf.Max(0f, attack.cooldown);
    }

    void UpdatePositioning(float distanceToPlayer)
    {
        currentState = BossState.Approach;

        Vector3 direction = player.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
        {
            StopMove();
            return;
        }

        float desiredDistance = GetDesiredCombatDistance(distanceToPlayer);
        if (Mathf.Abs(distanceToPlayer - desiredDistance) <= combatDistanceTolerance)
        {
            StopMove();
            return;
        }

        Vector3 desiredPosition = player.position - direction.normalized * desiredDistance;
        if (Time.time - lastRepathTime <= repathInterval)
            return;

        lastRepathTime = Time.time;
        agent.isStopped = false;
        agent.updatePosition = true;
        agent.SetDestination(desiredPosition);
    }

    float GetDesiredCombatDistance(float distanceToPlayer)
    {
        BossAttackDefinition spacingAttack = SelectSpacingAttack(distanceToPlayer);
        if (spacingAttack != null)
            return Mathf.Max(0f, spacingAttack.PreferredRange);

        return Mathf.Max(0f, approachStopDistance);
    }

    BossAttackDefinition SelectSpacingAttack(float distanceToPlayer)
    {
        if (attacks == null || attacks.Length == 0)
            return null;

        BossAttackDefinition bestAttack = null;
        int bestScore = int.MinValue;

        for (int i = 0; i < attacks.Length; i++)
        {
            BossAttackDefinition attack = attacks[i];
            if (attack == null)
                continue;

            int score = attack.priority * 1000;

            if (IsAttackReady(attack))
                score += 200;

            if (attack.IsInRange(distanceToPlayer))
                score += 100;

            score -= Mathf.RoundToInt(Mathf.Abs(attack.PreferredRange - distanceToPlayer) * 100f);

            if (score > bestScore)
            {
                bestScore = score;
                bestAttack = attack;
            }
        }

        return bestAttack;
    }

    void StartAttack(BossAttackDefinition attack)
    {
        if (attack == null)
            return;

        currentAttack = attack;
        currentAttackElapsed = 0f;
        currentAttackIndex = attack.attackIndex;
        currentAttackPhase = BossAttackPhase.Startup;
        currentAttackWindowIndex = 0;
        attackHitConfirmedThisWindow = false;
        rangedAttackResultPending = false;
        currentState = BossState.Attacking;

        SetAttackCooldown(attack);
        StopMove();
        if (staggerSystem != null)
            staggerSystem.ClearPendingHitReaction();

        SetWeaponTrailForAttack(attack);
        SetWeaponTrailActive(false);

        if (meleeDamageWindow != null)
            meleeDamageWindow.ForceCloseWindow();

        if (animatorController != null)
        {
            animatorController.SetTarget(player);
            animatorController.RequestAttack(attack);
            animatorController.SyncAttackPhase(currentAttackPhase);
        }
    }

    void UpdateAttackState()
    {
        if (animatorController == null)
            return;

        if (currentAttack == null)
        {
            FinishAttack();
            return;
        }

        currentAttackElapsed += Time.deltaTime;
        BossAttackPhase evaluatedPhase = currentAttack.EvaluatePhase(currentAttackElapsed);
        if (evaluatedPhase != currentAttackPhase)
            SetAttackPhase(evaluatedPhase);

        StopMove();

        if (!animatorController.IsBusyWithAttackMotion && !animatorController.IsInAttackState())
            FinishAttack();
    }

    void SetAttackPhase(BossAttackPhase phase)
    {
        currentAttackPhase = phase;

        if (animatorController != null)
            animatorController.SyncAttackPhase(phase);
    }

    void FinishAttack()
    {
        currentAttack = null;
        currentAttackElapsed = 0f;
        currentAttackIndex = -1;
        currentAttackPhase = BossAttackPhase.None;
        currentAttackWindowIndex = 0;
        attackHitConfirmedThisWindow = false;
        rangedAttackResultPending = false;

        if (meleeDamageWindow != null)
            meleeDamageWindow.ForceCloseWindow();

        SetWeaponTrailActive(false);
        currentState = BossState.Idle;
    }

    void StopMove()
    {
        if (agent == null || !agent.enabled)
            return;

        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

    public void ForceInterruptAction()
    {
        if (meleeDamageWindow != null)
            meleeDamageWindow.ForceCloseWindow();

        StopMove();

        if (animatorController != null)
            animatorController.AbortAttackMotion();

        if (staggerSystem != null)
            staggerSystem.ClearPendingHitReaction();

        SetWeaponTrailActive(false);
        currentAttack = null;
        currentAttackElapsed = 0f;
        currentAttackIndex = -1;
        currentAttackPhase = BossAttackPhase.None;
        currentAttackWindowIndex = 0;
        attackHitConfirmedThisWindow = false;
        rangedAttackResultPending = false;

        if (currentState != BossState.Dead)
            currentState = BossState.Idle;
    }

    public bool CanInterruptWithHitReaction()
    {
        if (currentState != BossState.Attacking || currentAttack == null)
            return true;

        return currentAttack.IsInterruptible(currentAttackPhase);
    }

    void Die()
    {
        currentState = BossState.Dead;
        attackHitConfirmedThisWindow = false;
        rangedAttackResultPending = false;

        if (meleeDamageWindow != null)
            meleeDamageWindow.ForceCloseWindow();

        SetWeaponTrailActive(false);
        StopMove();

        if (animatorController != null)
            animatorController.PlayDeath();

        if (staggerSystem != null)
            staggerSystem.ClearPendingHitReaction();

        enabled = false;
    }

    public void TakeDamage(float amount, GameObject attacker = null)
    {
        if (currentState == BossState.Dead)
            return;

        float safeAmount = Mathf.Max(0f, amount);
        if (safeAmount <= 0f)
            return;

        if (combatAudioController != null)
            combatAudioController.PlayHurt();

        currentHP = Mathf.Max(0f, currentHP - safeAmount);
        if (currentHP <= 0f)
            Die();
    }

    void SetWeaponTrailForAttack(BossAttackDefinition attack)
    {
        if (weaponTrailVFX == null || attack == null)
            return;

        weaponTrailVFX.SetTrailSet(GetTrailSetForAttack(attack.attackIndex));
    }

    void SetWeaponTrailActive(bool active)
    {
        if (weaponTrailVFX == null)
            return;

        if (active)
            weaponTrailVFX.TrailOn();
        else
            weaponTrailVFX.TrailOff();
    }

    BossWeaponTrailController.TrailSet GetTrailSetForAttack(int attackIndex)
    {
        switch (attackIndex)
        {
            case 3:
                return BossWeaponTrailController.TrailSet.MeleeSkill1;
            case 4:
                return BossWeaponTrailController.TrailSet.MeleeSkill2;
            case 5:
                return BossWeaponTrailController.TrailSet.MeleeSkill3;
            case 6:
                return BossWeaponTrailController.TrailSet.Ranged;
            default:
                return BossWeaponTrailController.TrailSet.Normal;
        }
    }

    public void AnimEvent_OpenMeleeWindow()
    {
        AnimEvent_OpenMeleeWindowSection(0);
    }

    public void AnimEvent_OpenMeleeWindowSection(int windowIndex)
    {
        if (staggerSystem != null && staggerSystem.ShouldLockBossAction())
            return;

        if (meleeDamageWindow == null || currentAttack == null || !currentAttack.opensMeleeWindow)
            return;

        currentAttackWindowIndex = Mathf.Max(0, windowIndex);
        attackHitConfirmedThisWindow = false;
        meleeDamageWindow.OpenWindow(currentAttackIndex, currentAttackWindowIndex);
        SetAttackPhase(BossAttackPhase.Active);
    }

    public void AnimEvent_CloseMeleeWindow()
    {
        if (meleeDamageWindow == null)
            return;

        if (!attackHitConfirmedThisWindow && combatAudioController != null)
            combatAudioController.PlayAttackMiss();

        meleeDamageWindow.CloseWindow();
        currentAttackWindowIndex = 0;
        attackHitConfirmedThisWindow = false;

        if (currentState == BossState.Attacking)
            SetAttackPhase(BossAttackPhase.Recovery);
    }

    public void AnimEvent_FireProjectile()
    {
        if (staggerSystem != null && staggerSystem.ShouldLockBossAction())
            return;

        if (rangedSkillCaster == null || currentAttack == null || !currentAttack.firesProjectile)
            return;

        SetAttackPhase(BossAttackPhase.Active);

        Vector3 attackDirection = animatorController != null
            ? animatorController.GetCurrentAttackDirection()
            : transform.forward;

        attackHitConfirmedThisWindow = false;
        rangedAttackResultPending = rangedSkillCaster.Fire(currentAttackIndex, attackDirection);
    }

    public void AnimEvent_LockAttackDirection()
    {
        if (animatorController == null)
            return;

        animatorController.LockCurrentAttackDirection();
    }

    public void AnimEvent_PlayAttackSFX()
    {
        if (combatAudioController == null)
            return;

        combatAudioController.PlayAttackStart();
    }

    public void NotifyAttackHitConfirmed()
    {
        if (currentState == BossState.Dead)
            return;

        if (attackHitConfirmedThisWindow)
            return;

        attackHitConfirmedThisWindow = true;
        rangedAttackResultPending = false;

        if (combatAudioController != null)
            combatAudioController.PlayAttackHit();
    }

    public void NotifyRangedAttackMiss()
    {
        if (currentState == BossState.Dead)
            return;

        if (!rangedAttackResultPending)
            return;

        rangedAttackResultPending = false;

        if (!attackHitConfirmedThisWindow && combatAudioController != null)
            combatAudioController.PlayAttackMiss();
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, engageDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, approachStopDistance);
    }

    static BossAttackDefinition[] CreateDefaultAttackSet()
    {
        return new[]
        {
            new BossAttackDefinition
            {
                displayName = "Normal 1",
                attackIndex = 0,
                category = BossAttackCategory.Normal,
                cooldown = 4f,
                minRange = 0f,
                maxRange = 4f,
                preferredDistance = 4f,
                priority = 10,
                startupDuration = 0.25f,
                activeDuration = 0.2f,
                recoveryDuration = 0.45f,
                alignBeforeAttack = true,
                useRootMotionPosition = true,
                useRootMotionRotation = true,
                trackTargetUntilDirectionLock = false,
                requireDirectionLockEvent = false,
                opensMeleeWindow = true,
                firesProjectile = false,
                interruptibleInStartup = false,
                interruptibleInActive = false,
                interruptibleInRecovery = true
            },
            new BossAttackDefinition
            {
                displayName = "Normal 2",
                attackIndex = 1,
                category = BossAttackCategory.Normal,
                cooldown = 4f,
                minRange = 0f,
                maxRange = 4f,
                preferredDistance = 4f,
                priority = 10,
                startupDuration = 0.28f,
                activeDuration = 0.2f,
                recoveryDuration = 0.42f,
                alignBeforeAttack = true,
                useRootMotionPosition = true,
                useRootMotionRotation = true,
                trackTargetUntilDirectionLock = false,
                requireDirectionLockEvent = false,
                opensMeleeWindow = true,
                firesProjectile = false,
                interruptibleInStartup = false,
                interruptibleInActive = false,
                interruptibleInRecovery = true
            },
            new BossAttackDefinition
            {
                displayName = "Normal 3",
                attackIndex = 2,
                category = BossAttackCategory.Normal,
                cooldown = 4f,
                minRange = 0f,
                maxRange = 4.5f,
                preferredDistance = 4f,
                priority = 11,
                startupDuration = 0.32f,
                activeDuration = 0.22f,
                recoveryDuration = 0.42f,
                alignBeforeAttack = true,
                useRootMotionPosition = true,
                useRootMotionRotation = true,
                trackTargetUntilDirectionLock = false,
                requireDirectionLockEvent = false,
                opensMeleeWindow = true,
                firesProjectile = false,
                interruptibleInStartup = false,
                interruptibleInActive = false,
                interruptibleInRecovery = true
            },
            new BossAttackDefinition
            {
                displayName = "Melee Skill 1",
                attackIndex = 3,
                category = BossAttackCategory.MeleeSkill,
                cooldown = 8f,
                minRange = 0f,
                maxRange = 6f,
                preferredDistance = 4.5f,
                priority = 20,
                startupDuration = 0.35f,
                activeDuration = 0.22f,
                recoveryDuration = 0.38f,
                alignBeforeAttack = true,
                useRootMotionPosition = true,
                useRootMotionRotation = false,
                trackTargetUntilDirectionLock = true,
                requireDirectionLockEvent = true,
                maxTurnAngle = 35f,
                turnSpeed = 360f,
                opensMeleeWindow = true,
                firesProjectile = false,
                interruptibleInStartup = false,
                interruptibleInActive = false,
                interruptibleInRecovery = true
            },
            new BossAttackDefinition
            {
                displayName = "Melee Skill 2",
                attackIndex = 4,
                category = BossAttackCategory.MeleeSkill,
                cooldown = 10f,
                minRange = 0f,
                maxRange = 6.5f,
                preferredDistance = 5f,
                priority = 21,
                startupDuration = 0.38f,
                activeDuration = 0.24f,
                recoveryDuration = 0.4f,
                alignBeforeAttack = true,
                useRootMotionPosition = true,
                useRootMotionRotation = false,
                trackTargetUntilDirectionLock = true,
                requireDirectionLockEvent = true,
                maxTurnAngle = 35f,
                turnSpeed = 360f,
                opensMeleeWindow = true,
                firesProjectile = false,
                interruptibleInStartup = false,
                interruptibleInActive = false,
                interruptibleInRecovery = true
            },
            new BossAttackDefinition
            {
                displayName = "Melee Skill 3",
                attackIndex = 5,
                category = BossAttackCategory.MeleeSkill,
                cooldown = 10f,
                minRange = 0f,
                maxRange = 6.5f,
                preferredDistance = 5f,
                priority = 22,
                startupDuration = 0.42f,
                activeDuration = 0.24f,
                recoveryDuration = 0.42f,
                alignBeforeAttack = true,
                useRootMotionPosition = true,
                useRootMotionRotation = false,
                trackTargetUntilDirectionLock = true,
                requireDirectionLockEvent = true,
                maxTurnAngle = 35f,
                turnSpeed = 360f,
                opensMeleeWindow = true,
                firesProjectile = false,
                interruptibleInStartup = false,
                interruptibleInActive = false,
                interruptibleInRecovery = true
            },
            new BossAttackDefinition
            {
                displayName = "Ranged",
                attackIndex = 6,
                category = BossAttackCategory.Ranged,
                cooldown = 15f,
                minRange = 4.5f,
                maxRange = 12f,
                preferredDistance = 8f,
                priority = 18,
                startupDuration = 0.4f,
                activeDuration = 0.2f,
                recoveryDuration = 0.45f,
                alignBeforeAttack = true,
                useRootMotionPosition = true,
                useRootMotionRotation = false,
                trackTargetUntilDirectionLock = true,
                requireDirectionLockEvent = true,
                maxTurnAngle = 35f,
                turnSpeed = 360f,
                opensMeleeWindow = false,
                firesProjectile = true,
                interruptibleInStartup = false,
                interruptibleInActive = false,
                interruptibleInRecovery = true
            }
        };
    }
}
