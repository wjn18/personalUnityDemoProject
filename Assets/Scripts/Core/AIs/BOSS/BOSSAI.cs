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

    public enum BossAttackType
    {
        Normal,
        MeleeSkill,
        Ranged
    }

    [System.Serializable]
    public class AttackConfig
    {
        public string displayName;
        public int attackIndex;
        public BossAttackType attackType;
        public float cooldown = 1f;
        public float attackRange = 3f;
    }

    [Header("Refs")]
    public Transform player;
    public NavMeshAgent agent;
    public BossAnimatorController animatorController;
    public BossMeleeDamageWindow meleeDamageWindow;
    public BossRangedSkillCaster rangedSkillCaster;
    public BossStaggerSystem staggerSystem;

    [Header("Stats")]
    public float maxHP = 1000f;
    public float currentHP = 1000f;

    [Header("Detection")]
    public float engageDistance = 25f;
    public float meleeDecisionDistance = 6f;

    [Header("Movement")]
    public float approachStopDistance = 3.5f;
    public float retreatDistance = 5f;
    public float repathInterval = 0.15f;

    [Header("Damage Retreat Rule")]
    public float damageWindow = 2f;
    public float retreatDamageThreshold = 40f;
    public float retreatCooldown = 4f;

    [Header("Runtime")]
    public BossState currentState = BossState.Idle;
    public int currentAttackIndex = -1;

    [Header("Attacks")]
    public AttackConfig[] attacks = new AttackConfig[]
    {
        new AttackConfig { displayName = "Normal 1", attackIndex = 0, attackType = BossAttackType.Normal, cooldown = 4.0f, attackRange = 4.0f },
        new AttackConfig { displayName = "Normal 2", attackIndex = 1, attackType = BossAttackType.Normal, cooldown = 4.0f, attackRange = 4.0f },
        new AttackConfig { displayName = "Normal 3", attackIndex = 2, attackType = BossAttackType.Normal, cooldown = 4.0f, attackRange = 4.0f },
        new AttackConfig { displayName = "Melee Skill 1", attackIndex = 3, attackType = BossAttackType.MeleeSkill, cooldown = 8.0f, attackRange = 6f },
        new AttackConfig { displayName = "Melee Skill 2", attackIndex = 4, attackType = BossAttackType.MeleeSkill, cooldown = 10.0f, attackRange = 6.5f },
        new AttackConfig { displayName = "Melee Skill 3", attackIndex = 5, attackType = BossAttackType.MeleeSkill, cooldown = 10.0f, attackRange = 6.5f },
        new AttackConfig { displayName = "Ranged", attackIndex = 6, attackType = BossAttackType.Ranged, cooldown = 15.0f, attackRange = 12.0f },
    };

    [Header("Debug")]
    public bool drawGizmos = true;

    readonly Dictionary<int, float> nextReadyTimeByIndex = new Dictionary<int, float>();
    readonly List<DamageRecord> damageRecords = new List<DamageRecord>();

    Vector3 retreatTarget;
    float lastRepathTime;
    float nextRetreatAllowedTime;

    struct DamageRecord
    {
        public float time;
        public float amount;
    }

    void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animatorController == null)
            animatorController = GetComponent<BossAnimatorController>();

        if (staggerSystem == null)
            staggerSystem = GetComponent<BossStaggerSystem>();

        agent.updateRotation = false;
        currentHP = Mathf.Clamp(currentHP, 0f, maxHP);

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
            StopMove();
            currentState = BossState.Idle;

            if (meleeDamageWindow != null)
                meleeDamageWindow.ForceCloseWindow();

            return;
        }

        CleanupOldDamageRecords();

        if (currentHP <= 0f)
        {
            Die();
            return;
        }

        if (currentState == BossState.Attacking)
        {
            UpdateAttackingState();
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > engageDistance)
        {
            StopMove();
            currentState = BossState.Idle;
            return;
        }

        if (ShouldRetreat())
            StartRetreat();

        if (currentState == BossState.Retreat)
        {
            UpdateRetreat();
            return;
        }

        DecideAction(distance);
    }

    void BuildCooldownTable()
    {
        nextReadyTimeByIndex.Clear();

        if (attacks == null) return;

        for (int i = 0; i < attacks.Length; i++)
        {
            if (attacks[i] == null) continue;

            if (!nextReadyTimeByIndex.ContainsKey(attacks[i].attackIndex))
                nextReadyTimeByIndex.Add(attacks[i].attackIndex, 0f);
        }
    }

    void DecideAction(float distanceToPlayer)
    {
        if (staggerSystem != null && staggerSystem.ShouldLockBossAction())
            return;

        if (distanceToPlayer < meleeDecisionDistance)
        {
            AttackConfig meleeSkill = PickRandomAvailableAttack(BossAttackType.MeleeSkill, distanceToPlayer);
            if (meleeSkill != null)
            {
                StartAttack(meleeSkill);
                return;
            }

            AttackConfig normalAttack = PickRandomAvailableAttack(BossAttackType.Normal, distanceToPlayer);
            if (normalAttack != null)
            {
                StartAttack(normalAttack);
                return;
            }

            ApproachPlayer();
            return;
        }

        AttackConfig rangedAttack = GetAttackByIndex(6);
        if (rangedAttack != null && IsAttackReady(rangedAttack) && distanceToPlayer <= rangedAttack.attackRange)
        {
            StartAttack(rangedAttack);
            return;
        }

        ApproachPlayer();
    }

    AttackConfig PickRandomAvailableAttack(BossAttackType type, float distanceToPlayer)
    {
        List<AttackConfig> candidates = new List<AttackConfig>();

        if (attacks == null)
            return null;

        for (int i = 0; i < attacks.Length; i++)
        {
            AttackConfig cfg = attacks[i];
            if (cfg == null) continue;
            if (cfg.attackType != type) continue;
            if (!IsAttackReady(cfg)) continue;
            if (distanceToPlayer > cfg.attackRange) continue;

            candidates.Add(cfg);
        }

        if (candidates.Count == 0)
            return null;

        return candidates[Random.Range(0, candidates.Count)];
    }

    AttackConfig GetAttackByIndex(int attackIndex)
    {
        if (attacks == null)
            return null;

        for (int i = 0; i < attacks.Length; i++)
        {
            if (attacks[i] != null && attacks[i].attackIndex == attackIndex)
                return attacks[i];
        }

        return null;
    }

    bool IsAttackReady(AttackConfig attack)
    {
        if (attack == null)
            return false;

        if (!nextReadyTimeByIndex.TryGetValue(attack.attackIndex, out float readyTime))
            return true;

        return Time.time >= readyTime;
    }

    void SetAttackCooldown(AttackConfig attack)
    {
        if (attack == null)
            return;

        nextReadyTimeByIndex[attack.attackIndex] = Time.time + Mathf.Max(0f, attack.cooldown);
    }

    void StartAttack(AttackConfig attack)
    {
        if (attack == null)
            return;

        if (staggerSystem != null && staggerSystem.ShouldLockBossAction())
            return;

        currentState = BossState.Attacking;
        currentAttackIndex = attack.attackIndex;
        SetAttackCooldown(attack);

        StopMove();

        if (meleeDamageWindow != null)
            meleeDamageWindow.ForceCloseWindow();

        animatorController.RequestAttack(attack.attackIndex);
    }

    void UpdateAttackingState()
    {
        if (animatorController == null)
            return;

        if (staggerSystem != null && staggerSystem.ShouldLockBossAction())
        {
            StopMove();
            currentState = BossState.Idle;

            if (meleeDamageWindow != null)
                meleeDamageWindow.ForceCloseWindow();

            return;
        }

        if (!animatorController.IsBusyWithAttackMotion && !animatorController.IsInAttackState())
        {
            currentState = BossState.Idle;
            currentAttackIndex = -1;

            if (meleeDamageWindow != null)
                meleeDamageWindow.ForceCloseWindow();

            return;
        }

        StopMove();
    }

    void ApproachPlayer()
    {
        if (staggerSystem != null && staggerSystem.ShouldLockBossAction())
            return;

        currentState = BossState.Approach;

        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        float dist = dir.magnitude;
        if (dist <= approachStopDistance)
        {
            StopMove();
            return;
        }

        Vector3 targetPos = player.position - dir.normalized * approachStopDistance;

        if (Time.time - lastRepathTime > repathInterval)
        {
            lastRepathTime = Time.time;
            agent.isStopped = false;
            agent.SetDestination(targetPos);
        }
    }

    bool ShouldRetreat()
    {
        if (currentState == BossState.Retreat)
            return false;

        if (Time.time < nextRetreatAllowedTime)
            return false;

        float recentDamage = GetRecentDamageTotal();
        return recentDamage >= retreatDamageThreshold;
    }

    void StartRetreat()
    {
        if (staggerSystem != null && staggerSystem.ShouldLockBossAction())
            return;

        nextRetreatAllowedTime = Time.time + retreatCooldown;
        ClearDamageWindow();

        Vector3 away = transform.position - player.position;
        away.y = 0f;

        if (away.sqrMagnitude < 0.001f)
            away = -transform.forward;

        away.Normalize();
        retreatTarget = transform.position + away * retreatDistance;

        if (NavMesh.SamplePosition(retreatTarget, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            retreatTarget = hit.position;

        agent.isStopped = false;
        agent.SetDestination(retreatTarget);
        currentState = BossState.Retreat;
    }

    void UpdateRetreat()
    {
        if (staggerSystem != null && staggerSystem.ShouldLockBossAction())
        {
            StopMove();
            currentState = BossState.Idle;
            return;
        }

        if (Time.time - lastRepathTime > repathInterval)
        {
            lastRepathTime = Time.time;
            agent.SetDestination(retreatTarget);
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.15f)
        {
            StopMove();
            currentState = BossState.Idle;
        }
    }

    void StopMove()
    {
        if (agent == null || !agent.enabled)
            return;

        agent.isStopped = true;
        agent.ResetPath();
    }

    void Die()
    {
        currentState = BossState.Dead;

        if (meleeDamageWindow != null)
            meleeDamageWindow.ForceCloseWindow();

        StopMove();
        animatorController.PlayDeath();
        enabled = false;
    }

    public void TakeDamage(float amount, GameObject attacker = null)
    {
        if (currentState == BossState.Dead)
            return;

        float dmg = Mathf.Max(0f, amount);
        currentHP = Mathf.Max(0f, currentHP - dmg);

        damageRecords.Add(new DamageRecord
        {
            time = Time.time,
            amount = dmg
        });

        if (currentHP <= 0f)
            Die();
    }

    float GetRecentDamageTotal()
    {
        float total = 0f;
        float minTime = Time.time - damageWindow;

        for (int i = 0; i < damageRecords.Count; i++)
        {
            if (damageRecords[i].time >= minTime)
                total += damageRecords[i].amount;
        }

        return total;
    }

    void CleanupOldDamageRecords()
    {
        float minTime = Time.time - damageWindow;

        for (int i = damageRecords.Count - 1; i >= 0; i--)
        {
            if (damageRecords[i].time < minTime)
                damageRecords.RemoveAt(i);
        }
    }

    void ClearDamageWindow()
    {
        damageRecords.Clear();
    }

    public void AnimEvent_OpenMeleeWindow()
    {
        if (staggerSystem != null && staggerSystem.ShouldLockBossAction())
            return;

        if (meleeDamageWindow == null)
            return;

        meleeDamageWindow.OpenWindow(currentAttackIndex);
    }

    public void AnimEvent_CloseMeleeWindow()
    {
        if (meleeDamageWindow == null)
            return;

        meleeDamageWindow.CloseWindow();
    }

    public void AnimEvent_FireProjectile()
    {
        if (staggerSystem != null && staggerSystem.ShouldLockBossAction())
            return;

        if (rangedSkillCaster == null)
            return;

        rangedSkillCaster.FireAtPlayer(currentAttackIndex);
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, meleeDecisionDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, engageDistance);

        if (currentState == BossState.Retreat)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(retreatTarget, 0.2f);
            Gizmos.DrawLine(transform.position, retreatTarget);
        }
    }
}