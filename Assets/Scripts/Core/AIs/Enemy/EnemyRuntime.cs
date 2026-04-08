using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyRuntime : MonoBehaviour, IDamageable
{
    [Header("Config")]
    public EnemyConfig config;

    [Header("Runtime")]
    public float hp;
    [SerializeField] private bool isDead = false;

    [Header("Death")]
    public float destroyDelay = 5f;

    public event Action<float, float> OnHPChanged;
    public event Action OnDied;

    public GameObject LastAttacker { get; private set; }

    private EnemyAI enemyAI;
    private NavMeshAgent agent;
    private Animator anim;
    private EnemyAttackHit[] attackHits;
    private Collider[] allColliders;

    void Awake()
    {
        enemyAI = GetComponent<EnemyAI>();
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        attackHits = GetComponentsInChildren<EnemyAttackHit>(true);
        allColliders = GetComponentsInChildren<Collider>(true);

        if (config == null)
        {
            Debug.LogError($"{name}: EnemyRuntime Ã»ÓÐ°ó¶¨ config!");
            hp = 1f;
            return;
        }

        hp = config.maxHP;
        OnHPChanged?.Invoke(hp, config.maxHP);
    }

    public bool IsDead()
    {
        return isDead;
    }

    public void TakeDamage(float amount, GameObject attacker = null)
    {
        if (config == null || isDead) return;

        if (attacker != null)
            LastAttacker = attacker;

        hp = Mathf.Max(0f, hp - Mathf.Max(0f, amount));
        OnHPChanged?.Invoke(hp, config.maxHP);

        if (hp <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        hp = 0f;

        GetComponent<LockOnTarget>()?.NotifyDied();

        OnDied?.Invoke();

        EnterFakeDeath();

        StartCoroutine(DestroyAfterDelay());
    }

    void EnterFakeDeath()
    {
        if (enemyAI != null)
        {
            enemyAI.SetDead(true);
        }

        if (agent != null)
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
            }
        }

        if (attackHits != null)
        {
            foreach (var hit in attackHits)
            {
                if (hit != null)
                    hit.enabled = false;
            }
        }

        if (allColliders != null)
        {
            foreach (var col in allColliders)
            {
                if (col != null)
                    col.enabled = false;
            }
        }

        Rigidbody[] rbs = GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in rbs)
        {
            if (rb == null) continue;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (anim != null)
        {
            anim.speed = 1f;
            anim.updateMode = AnimatorUpdateMode.Normal;
            anim.applyRootMotion = false;
        }
    }

    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}