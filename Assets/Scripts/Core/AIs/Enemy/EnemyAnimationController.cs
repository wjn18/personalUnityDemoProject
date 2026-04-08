using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(EnemyRuntime))]
public class EnemyAnimationController : MonoBehaviour
{
    [Header("Animator Params")]
    public string speedParam = "Speed";
    public string attackTrigger = "Attack";
    public string hitTrigger = "Hit";
    public string heavyHitTrigger = "HeavyHit";
    public string isDeadTrigger = "IsDead";

    [Header("State Names")]
    public string deathStateName = "Falling Forward Death";
    public string flyingBackDeathStateName = "Flying Back Death";
    public string standingUpStateName = "Standing Up";

    [Header("Speed Smooth")]
    public float dampTime = 0.1f;

    [Header("Hit Reaction")]
    public float hitReactDuration = 1.2f;
    public float hitResetDuration = 3.0f;
    public float hitStopDuration = 0.35f;
    public bool stopAgentOnHit = true;

    [Header("Heavy Hit Reaction")]
    public float heavyHitStopDuration = 1.0f;

    private Animator anim;
    private NavMeshAgent agent;
    private EnemyAI enemyAI;
    private EnemyRuntime enemyRuntime;

    private float lastHP;
    private bool initializedHP = false;

    private bool isDead = false;
    private bool hitWindowActive = false;
    private bool isSuperArmor = false;
    private bool isInHitReact = false;
    private bool isInHeavyHitReact = false;
    private bool isInFlyingBackDeath = false;
    private bool isInStandingUp = false;

    private float hitWindowTimer = 0f;
    private float hitReactTimer = 0f;

    private bool suppressNextDamageReaction = false;

    public bool IsDead => isDead;
    public bool IsInHitReact => isInHitReact;
    public bool IsInHeavyHitReact => isInHeavyHitReact;
    public bool IsInFlyingBackDeath => isInFlyingBackDeath;
    public bool IsInStandingUp => isInStandingUp;

    void Awake()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        enemyAI = GetComponent<EnemyAI>();
        enemyRuntime = GetComponent<EnemyRuntime>();
    }

    void OnEnable()
    {
        if (enemyAI != null)
            enemyAI.OnAttack += HandleAttack;

        if (enemyRuntime != null)
            enemyRuntime.OnDied += HandleDied;
    }

    void OnDisable()
    {
        if (enemyAI != null)
            enemyAI.OnAttack -= HandleAttack;

        if (enemyRuntime != null)
            enemyRuntime.OnDied -= HandleDied;
    }

    void Start()
    {
        if (enemyRuntime != null)
        {
            lastHP = enemyRuntime.hp;
            initializedHP = true;
        }
    }

    void Update()
    {
        if (anim == null) return;

        if (isDead)
            return;

        UpdateAnimatorStateFlags();
        UpdateHPState();
        UpdateHitState();
        UpdateMoveAnimation();
    }

    void UpdateAnimatorStateFlags()
    {
        isInFlyingBackDeath = IsInStateOrTransition(flyingBackDeathStateName);
        isInStandingUp = IsInStateOrTransition(standingUpStateName);
    }

    bool IsInStateOrTransition(string stateName)
    {
        if (anim == null || string.IsNullOrEmpty(stateName))
            return false;

        AnimatorStateInfo current = anim.GetCurrentAnimatorStateInfo(0);

        if (anim.IsInTransition(0))
        {
            AnimatorStateInfo next = anim.GetNextAnimatorStateInfo(0);
            return current.IsName(stateName) || next.IsName(stateName);
        }

        return current.IsName(stateName);
    }

    void UpdateHPState()
    {
        if (enemyRuntime == null) return;

        if (!initializedHP)
        {
            lastHP = enemyRuntime.hp;
            initializedHP = true;
        }

        float currentHP = enemyRuntime.hp;

        if (currentHP < lastHP)
        {
            if (suppressNextDamageReaction)
            {
                suppressNextDamageReaction = false;
            }
            else
            {
                OnDamaged();
            }
        }

        lastHP = currentHP;
    }

    void UpdateHitState()
    {
        if (hitWindowActive)
        {
            hitWindowTimer += Time.deltaTime;

            if (!isSuperArmor && hitWindowTimer >= hitReactDuration)
                isSuperArmor = true;

            if (hitWindowTimer >= hitResetDuration)
                ResetHitWindow();
        }

        if (isInHitReact)
        {
            hitReactTimer -= Time.deltaTime;

            if (hitReactTimer <= 0f)
            {
                isInHitReact = false;
                isInHeavyHitReact = false;

                if (agent != null && agent.enabled && agent.isOnNavMesh && !isDead && !isInFlyingBackDeath && !isInStandingUp)
                    agent.isStopped = false;
            }
        }
    }

    void UpdateMoveAnimation()
    {
        float speed = 0f;

        if (agent != null && agent.enabled)
            speed = agent.velocity.magnitude;

        anim.SetFloat(speedParam, speed, dampTime, Time.deltaTime);
    }

    void HandleAttack()
    {
        if (anim == null || isDead) return;
        if (isInHeavyHitReact) return;
        if (isInFlyingBackDeath) return;
        if (isInStandingUp) return;

        anim.ResetTrigger(hitTrigger);
        anim.ResetTrigger(heavyHitTrigger);
        anim.SetTrigger(attackTrigger);
    }

    void OnDamaged()
    {
        if (isDead) return;
        if (isInHeavyHitReact) return;
        if (isInFlyingBackDeath) return;
        if (isInStandingUp) return;

        if (!hitWindowActive)
        {
            hitWindowActive = true;
            hitWindowTimer = 0f;
            isSuperArmor = false;
        }

        if (isSuperArmor) return;

        PlayHitReaction();
    }

    void PlayHitReaction()
    {
        if (anim == null || isDead) return;
        if (isInHeavyHitReact) return;
        if (isInFlyingBackDeath) return;
        if (isInStandingUp) return;

        anim.ResetTrigger(attackTrigger);
        anim.ResetTrigger(heavyHitTrigger);
        anim.SetTrigger(hitTrigger);

        isInHitReact = true;
        isInHeavyHitReact = false;
        hitReactTimer = hitStopDuration;

        StopAgentImmediately();
    }

    public void PlayHeavyHitReaction()
    {
        if (anim == null || isDead) return;

        suppressNextDamageReaction = true;

        isInHitReact = true;
        isInHeavyHitReact = true;
        hitReactTimer = heavyHitStopDuration;

        hitWindowActive = false;
        isSuperArmor = false;
        hitWindowTimer = 0f;

        anim.ResetTrigger(attackTrigger);
        anim.ResetTrigger(hitTrigger);
        anim.ResetTrigger(heavyHitTrigger);
        anim.SetTrigger(heavyHitTrigger);

        StopAgentImmediately();
    }

    void StopAgentImmediately()
    {
        if (!stopAgentOnHit) return;
        if (agent == null || !agent.enabled || !agent.isOnNavMesh) return;

        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

    void HandleDied()
    {
        EnterDeath();
    }

    void EnterDeath()
    {
        if (isDead) return;
        isDead = true;

        isInHitReact = false;
        isInHeavyHitReact = false;
        isInFlyingBackDeath = false;
        isInStandingUp = false;
        isSuperArmor = false;
        hitWindowActive = false;
        hitWindowTimer = 0f;
        hitReactTimer = 0f;
        suppressNextDamageReaction = false;

        if (anim == null) return;

        anim.ResetTrigger(attackTrigger);
        anim.ResetTrigger(hitTrigger);
        anim.ResetTrigger(heavyHitTrigger);

        anim.SetTrigger(isDeadTrigger);

        anim.speed = 1f;
        anim.updateMode = AnimatorUpdateMode.Normal;
        anim.applyRootMotion = false;

        anim.Play(deathStateName, 0, 0f);
        anim.Update(0f);
    }

    void ResetHitWindow()
    {
        hitWindowActive = false;
        isSuperArmor = false;
        hitWindowTimer = 0f;
    }
}