using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GuardAI : MonoBehaviour
{
    public enum GuardState
    {
        Idle,
        Follow,
        Chase,
        Attack,
        Cooldown
    }

    [Header("Config")]
    public GuardConfig config;

    [Header("Refs")]
    public Transform player;

    [Header("Follow")]
    public float followDistance = 12f;          // 和玩家保持的理想距离
    public float followSlack = 1f;             // 允许的浮动范围，避免频繁抖动
    public float repathInterval = 0.25f;         // 跟随重算路径间隔
    public float followPointRefreshDistance = 1.0f; // 玩家移动这么多后，再更新站位点
    public float orbitJitter = 0.6f;             // 站位点的小随机偏移
    public float teleportBackDistance = 8f;      // 离玩家太远时，优先快速回位
    

    [Header("Detect")]
    public float detectRange = 15f;
    public float loseTargetRange = 20f;
    public string enemyTag = "Enemy";

    [Header("Move")]
    public float chaseStopDistance = 1.8f;       // 追敌时停止距离
    public float followStopDistance = 0.15f;     // 到达跟随点时的停止距离
    public float rotateSpeed = 10f;
    public float slowSpeed = 2f;  //靠近敌人后的移动速度
    public float slowDownDistance = 3.5f; // 开始减速的距离

    [Header("Attack")]
    public float attackRange = 2.2f;
    public float attackCooldown = 1.0f;
    public float attackHitTolerance = 0.5f;
    public float damage = 10f;
   

    [Header("Avoidance")]
    public bool setAvoidancePriorityAutomatically = true;
    [Range(0, 99)] public int avoidancePriority = 40;

    [Header("Debug")]
    public GuardState currentState = GuardState.Idle;
    public Transform currentTarget;
    public Transform attackerTarget;
    public bool drawGizmos = true;

    private NavMeshAgent agent;
    private float lastAttackTime = -999f;
    private float repathTimer = 0f;
    private bool damageAppliedThisAttack = false;

    private EnemyRuntime lastAttackerEnemy;
    private EnemyRuntime lockedEnemyRuntime;
    private Transform pendingAttackTarget;

    private Vector3 currentFollowPoint;
    private Vector3 lastPlayerPosForFollow;
    private bool hasFollowPoint = false;

    public float CurrentSpeed { get; private set; }

    private float normalSpeed;
    public bool IsChasing { get; private set; }

    public event Action OnAttack;

    // 敌人 -> 当前锁定它的 Guard
    private static Dictionary<EnemyRuntime, GuardAI> targetReservations =
        new Dictionary<EnemyRuntime, GuardAI>();

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        normalSpeed = agent.speed;

        if (player == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag("Player");
            if (obj != null)
                player = obj.transform;
        }

        agent.autoBraking = true;
        agent.stoppingDistance = followStopDistance;

        if (setAvoidancePriorityAutomatically)
        {
            agent.avoidancePriority = avoidancePriority;
        }

        currentState = GuardState.Idle;

        if (player != null)
        {
            lastPlayerPosForFollow = player.position;
        }
    }

    void Update()
    {
        if (player == null || agent == null || !agent.isOnNavMesh)
        {
            CurrentSpeed = 0f;
            return;
        }

        CleanupInvalidTargets();

        EnemyRuntime chosenEnemy = ChooseTargetEnemy();
        UpdateReservation(chosenEnemy);

        currentTarget = lockedEnemyRuntime != null ? lockedEnemyRuntime.transform : null;

        float distanceToTarget = DistanceToCurrentTargetXZ();
        float distanceToPlayer = DistanceToPlayerXZ();

        switch (currentState)
        {
            case GuardState.Idle:
                UpdateIdle(distanceToPlayer);
                break;

            case GuardState.Follow:
                UpdateFollow(distanceToPlayer);
                break;

            case GuardState.Chase:
                UpdateChase(distanceToTarget, distanceToPlayer);
                break;

            case GuardState.Attack:
                UpdateAttack(distanceToTarget, distanceToPlayer);
                break;

            case GuardState.Cooldown:
                UpdateCooldown(distanceToTarget, distanceToPlayer);
                break;
        }

        CurrentSpeed = agent.velocity.magnitude;
    }

    void UpdateIdle(float distanceToPlayer)
    {
        IsChasing = false;
        StopAgentCompletely();

        if (currentTarget != null)
        {
            currentState = GuardState.Chase;
            return;
        }

        currentState = GuardState.Follow;
    }

    void UpdateFollow(float distanceToPlayer)
    {
        IsChasing = false;
        agent.speed = normalSpeed;

        if (currentTarget != null)
        {
            currentState = GuardState.Chase;
            return;
        }

        agent.stoppingDistance = followStopDistance;

        float minKeepDistance = Mathf.Max(6.0f, followDistance - followSlack);
        float maxKeepDistance = followDistance + followSlack;

        // 在可接受范围内，就停下，不反复挤玩家
        if (distanceToPlayer >= minKeepDistance && distanceToPlayer <= maxKeepDistance)
        {
            StopAgentButKeepRotation();
            FaceSameDirectionAsPlayer();
            return;
        }

        // 太近了，立刻退到合适站位
        if (distanceToPlayer < minKeepDistance)
        {
            Vector3 retreatPoint = GetRetreatPointFromPlayer(minKeepDistance + 0.35f);
            MoveToFollowPoint(retreatPoint);
            return;
        }

        // 太远则重新计算护卫站位点
        repathTimer -= Time.deltaTime;
        bool playerMovedEnough = (player.position - lastPlayerPosForFollow).sqrMagnitude >=
                                 followPointRefreshDistance * followPointRefreshDistance;

        bool shouldRebuildFollowPoint =
            !hasFollowPoint ||
            repathTimer <= 0f ||
            playerMovedEnough ||
            distanceToPlayer > teleportBackDistance;

        if (shouldRebuildFollowPoint)
        {
            repathTimer = repathInterval;
            lastPlayerPosForFollow = player.position;

            Vector3 followPoint = BuildSmartFollowPoint();
            currentFollowPoint = followPoint;
            hasFollowPoint = true;
        }

        MoveToFollowPoint(currentFollowPoint);
    }

    void UpdateChase(float distanceToTarget, float distanceToPlayer)
    {
        if (currentTarget == null)
        {
            hasFollowPoint = false;
            currentState = GuardState.Follow;
            return;
        }

        if (distanceToTarget > loseTargetRange)
        {
            ReleaseCurrentReservation();
            hasFollowPoint = false;
            currentState = GuardState.Follow;
            return;
        }

        if (distanceToTarget <= attackRange)
        {
            IsChasing = false;
            StopAgentCompletely();
            currentState = GuardState.Attack;
            return;
        }

        if (distanceToTarget <= slowDownDistance)
        {
            agent.speed = slowSpeed;   // 靠近后慢下来
        }

        IsChasing = true;
        agent.isStopped = false;
        agent.stoppingDistance = chaseStopDistance;
        agent.SetDestination(currentTarget.position);
    }

    void UpdateAttack(float distanceToTarget, float distanceToPlayer)
    {
        if (currentTarget == null)
        {
            hasFollowPoint = false;
            currentState = GuardState.Follow;
            return;
        }

        IsChasing = false;
        StopAgentCompletely();
        FaceTarget(currentTarget.position);

        if (distanceToTarget > attackRange)
        {
            currentState = GuardState.Chase;
            return;
        }

        TryAttack();
    }

    void UpdateCooldown(float distanceToTarget, float distanceToPlayer)
    {
        if (currentTarget == null)
        {
            hasFollowPoint = false;
            currentState = GuardState.Follow;
            return;
        }

        IsChasing = false;
        StopAgentCompletely();
        FaceTarget(currentTarget.position);

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            if (distanceToTarget <= attackRange)
                currentState = GuardState.Attack;
            else if (distanceToTarget <= loseTargetRange)
                currentState = GuardState.Chase;
            else
            {
                hasFollowPoint = false;
                currentState = GuardState.Follow;
            }
        }
    }

    void TryAttack()
    {
        if (currentTarget == null) return;
        if (Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;
        damageAppliedThisAttack = false;
        pendingAttackTarget = currentTarget;

        OnAttack?.Invoke();
        currentState = GuardState.Cooldown;
    }

    public void ApplyAttackDamageNow()
    {
        if (damageAppliedThisAttack) return;
        if (pendingAttackTarget == null) return;

        EnemyRuntime er = pendingAttackTarget.GetComponent<EnemyRuntime>();
        if (er != null && er.IsDead()) return;

        float distance = Vector3.Distance(transform.position, pendingAttackTarget.position);
        if (distance > attackRange + attackHitTolerance)
            return;

        IDamageable damageable = pendingAttackTarget.GetComponent<IDamageable>();
        if (damageable == null) return;

        damageAppliedThisAttack = true;
        damageable.TakeDamage(damage, gameObject);
    }

    Vector3 BuildSmartFollowPoint()
    {
        Vector3 toGuard = transform.position - player.position;
        toGuard.y = 0f;

        Vector3 baseDir;

        if (toGuard.sqrMagnitude > 0.01f)
        {
            baseDir = toGuard.normalized;
        }
        else
        {
            baseDir = -player.forward;
            baseDir.y = 0f;
            if (baseDir.sqrMagnitude < 0.01f)
                baseDir = Vector3.back;
            baseDir.Normalize();
        }

        // 尝试多个候选点，选一个既在玩家周围，又不会太贴脸的位置
        const int maxTry = 5;
        Vector3 bestPoint = player.position + baseDir * followDistance;
        float bestScore = float.MaxValue;
        bool found = false;

        for (int i = 0; i < maxTry; i++)
        {
            float angleOffset = UnityEngine.Random.Range(-55f, 55f);
            Vector3 dir = Quaternion.Euler(0f, angleOffset, 0f) * baseDir;

            float radius = UnityEngine.Random.Range(followDistance - 0.2f, followDistance + 0.4f);
            Vector3 rawPoint = player.position + dir * radius;

            Vector2 jitter2D = UnityEngine.Random.insideUnitCircle * orbitJitter;
            rawPoint += new Vector3(jitter2D.x, 0f, jitter2D.y);

            Vector3 fromPlayer = rawPoint - player.position;
            fromPlayer.y = 0f;

            if (fromPlayer.magnitude < followDistance - followSlack)
            {
                fromPlayer = fromPlayer.normalized * (followDistance - followSlack);
                rawPoint = player.position + fromPlayer;
            }

            NavMeshHit hit;
            if (!NavMesh.SamplePosition(rawPoint, out hit, 2f, NavMesh.AllAreas))
                continue;

            float distToIdeal = Mathf.Abs(Vector3.Distance(hit.position, player.position) - followDistance);
            float moveCost = Vector3.Distance(transform.position, hit.position);
            float score = distToIdeal * 2f + moveCost * 0.35f;

            if (score < bestScore)
            {
                bestScore = score;
                bestPoint = hit.position;
                found = true;
            }
        }

        if (!found)
        {
            NavMeshHit hit;
            Vector3 fallback = player.position + baseDir * followDistance;
            if (NavMesh.SamplePosition(fallback, out hit, 2f, NavMesh.AllAreas))
                bestPoint = hit.position;
            else
            {
                Vector3 fallbackNearPlayer = player.position + baseDir * Mathf.Min(followDistance, 3f);

                if (NavMesh.SamplePosition(fallbackNearPlayer, out hit, 4f, NavMesh.AllAreas))
                    bestPoint = hit.position;
                
            }
        }

        return bestPoint;
    }

    Vector3 GetRetreatPointFromPlayer(float retreatDistance)
    {
        Vector3 dir = transform.position - player.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f)
        {
            dir = -player.forward;
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.01f)
                dir = Vector3.back;
        }

        dir.Normalize();

        Vector3 rawPoint = player.position + dir * retreatDistance;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(rawPoint, out hit, 2f, NavMesh.AllAreas))
            return hit.position;

        return transform.position;
    }

    void MoveToFollowPoint(Vector3 point)
    {
        float distToPoint = Vector3.Distance(transform.position, point);

        if (distToPoint <= followStopDistance + 0.05f)
        {
            StopAgentButKeepRotation();
            FaceSameDirectionAsPlayer();
            return;
        }

        agent.isStopped = false;
        agent.stoppingDistance = followStopDistance;
        agent.SetDestination(point);
    }

    void StopAgentCompletely()
    {
        if (agent == null) return;

        agent.isStopped = true;
        if (agent.hasPath)
            agent.ResetPath();
    }

    void StopAgentButKeepRotation()
    {
        if (agent == null) return;

        agent.isStopped = true;
        if (agent.hasPath)
            agent.ResetPath();
    }

    void FaceTarget(Vector3 targetPos)
    {
        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * rotateSpeed
            );
        }
    }

    void FaceSameDirectionAsPlayer()
    {
        if (player == null) return;

        Vector3 dir = player.forward;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Time.deltaTime * rotateSpeed * 0.6f
            );
        }
    }

    EnemyRuntime ChooseTargetEnemy()
    {
        // 1. 最高优先级：反击打我的敌人
        if (lastAttackerEnemy != null && IsEnemyValid(lastAttackerEnemy))
            return lastAttackerEnemy;

        // 2. 攻击正在攻击 player 的敌人
        EnemyRuntime attackingPlayer = FindEnemyAttackingPlayer();
        if (attackingPlayer != null)
            return attackingPlayer;

        // 3. 打自己范围内离 player 最近的敌人
        EnemyRuntime nearestToPlayer = FindNearestEnemyToPlayerInRange();
        if (nearestToPlayer != null)
            return nearestToPlayer;

        return null;
    }

    EnemyRuntime FindEnemyAttackingPlayer()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        EnemyRuntime best = null;
        float bestDist = float.MaxValue;

        foreach (GameObject go in enemies)
        {
            if (!go.activeInHierarchy) continue;

            EnemyRuntime enemyRuntime = go.GetComponent<EnemyRuntime>();
            EnemyAI enemyAI = go.GetComponent<EnemyAI>();

            if (enemyRuntime == null || enemyAI == null) continue;
            if (enemyRuntime.IsDead()) continue;
            if (enemyAI.currentTarget != player) continue;
            if (IsReservedByOther(enemyRuntime)) continue;

            float d = Vector3.Distance(transform.position, go.transform.position);
            if (d <= detectRange && d < bestDist)
            {
                bestDist = d;
                best = enemyRuntime;
            }
        }

        return best;
    }

    EnemyRuntime FindNearestEnemyToPlayerInRange()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);

        EnemyRuntime best = null;
        float bestDistToPlayer = float.MaxValue;

        foreach (GameObject go in enemies)
        {
            if (!go.activeInHierarchy) continue;

            EnemyRuntime enemyRuntime = go.GetComponent<EnemyRuntime>();
            if (enemyRuntime == null) continue;
            if (enemyRuntime.IsDead()) continue;
            if (IsReservedByOther(enemyRuntime)) continue;

            float distToSelf = Vector3.Distance(transform.position, go.transform.position);
            if (distToSelf > detectRange) continue;

            float distToPlayer = Vector3.Distance(player.position, go.transform.position);
            if (distToPlayer < bestDistToPlayer)
            {
                bestDistToPlayer = distToPlayer;
                best = enemyRuntime;
            }
        }

        return best;
    }

    void CleanupInvalidTargets()
    {
        if (lastAttackerEnemy != null)
        {
            if (!lastAttackerEnemy.gameObject.activeInHierarchy || lastAttackerEnemy.IsDead())
            {
                lastAttackerEnemy = null;
                attackerTarget = null;
            }
            else
            {
                attackerTarget = lastAttackerEnemy.transform;
            }
        }
        else
        {
            attackerTarget = null;
        }

        if (lockedEnemyRuntime != null)
        {
            if (!lockedEnemyRuntime.gameObject.activeInHierarchy || lockedEnemyRuntime.IsDead())
            {
                ReleaseCurrentReservation();
            }
        }

        if (pendingAttackTarget != null)
        {
            EnemyRuntime er = pendingAttackTarget.GetComponent<EnemyRuntime>();
            if (!pendingAttackTarget.gameObject.activeInHierarchy || (er != null && er.IsDead()))
            {
                pendingAttackTarget = null;
                damageAppliedThisAttack = false;
            }
        }
    }

    bool IsEnemyValid(EnemyRuntime enemy)
    {
        if (enemy == null) return false;
        if (!enemy.gameObject.activeInHierarchy) return false;
        if (enemy.IsDead()) return false;
        return true;
    }

    void UpdateReservation(EnemyRuntime newEnemy)
    {
        if (newEnemy == lockedEnemyRuntime)
            return;

        ReleaseCurrentReservation();

        if (newEnemy == null)
            return;

        bool isCounterAttackTarget = (newEnemy == lastAttackerEnemy);

        if (isCounterAttackTarget)
        {
            targetReservations[newEnemy] = this;
            lockedEnemyRuntime = newEnemy;
            return;
        }

        if (!targetReservations.TryGetValue(newEnemy, out GuardAI owner) || owner == null || owner == this)
        {
            targetReservations[newEnemy] = this;
            lockedEnemyRuntime = newEnemy;
        }
    }

    void ReleaseCurrentReservation()
    {
        if (lockedEnemyRuntime != null)
        {
            if (targetReservations.TryGetValue(lockedEnemyRuntime, out GuardAI owner))
            {
                if (owner == this)
                {
                    targetReservations.Remove(lockedEnemyRuntime);
                }
            }
        }

        lockedEnemyRuntime = null;
        currentTarget = null;
    }

    bool IsReservedByOther(EnemyRuntime enemy)
    {
        if (enemy == null) return true;

        if (targetReservations.TryGetValue(enemy, out GuardAI owner))
        {
            return owner != null && owner != this;
        }

        return false;
    }

    float DistanceToCurrentTargetXZ()
    {
        if (currentTarget == null)
            return float.MaxValue;

        Vector3 a = transform.position;
        Vector3 b = currentTarget.position;
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    float DistanceToPlayerXZ()
    {
        if (player == null)
            return float.MaxValue;

        Vector3 a = transform.position;
        Vector3 b = player.position;
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    public void NotifyBeingAttacked(EnemyRuntime attacker)
    {
        if (attacker == null) return;
        if (attacker.IsDead()) return;

        lastAttackerEnemy = attacker;
        attackerTarget = attacker.transform;

        if (currentState == GuardState.Follow || currentState == GuardState.Idle)
            currentState = GuardState.Chase;
    }

    void OnDisable()
    {
        ReleaseCurrentReservation();
    }

    void OnDestroy()
    {
        ReleaseCurrentReservation();
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, loseTargetRange);

        if (player != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(player.position, followDistance);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(player.position, Mathf.Max(0.1f, followDistance - followSlack));

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(player.position, followDistance + followSlack);

            if (hasFollowPoint)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(currentFollowPoint, 0.15f);
                Gizmos.DrawLine(transform.position, currentFollowPoint);
            }
        }

        if (currentTarget != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
}