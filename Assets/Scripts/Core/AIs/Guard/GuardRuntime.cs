using System;
using UnityEngine;

public class GuardRuntime : MonoBehaviour, IDamageable
{
    [Header("Config")]
    public GuardConfig config;

    [Header("Runtime")]
    public float hp;
    public bool isDead = false;

    private GuardAI guardAI;

    public event Action<float, float> OnHPChanged;
    public event Action OnDied;

    void Awake()
    {
        guardAI = GetComponent<GuardAI>();

        if (config == null)
        {
            Debug.LogError($"{name}: GuardRuntime Ã»ÓÐ°ó¶¨ config!");
            hp = 1f;
            return;
        }

        hp = config.maxHP;
        OnHPChanged?.Invoke(hp, config.maxHP);
    }

    public void TakeDamage(float amount, GameObject attacker = null)
    {
        if (isDead) return;

        hp = Mathf.Max(0f, hp - Mathf.Max(0f, amount));
        OnHPChanged?.Invoke(hp, config != null ? config.maxHP : 1f);

        if (attacker != null && guardAI != null)
        {
            EnemyRuntime er = attacker.GetComponent<EnemyRuntime>();
            if (er != null)
            {
                guardAI.NotifyBeingAttacked(er);
            }
        }

        if (hp <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;
        OnDied?.Invoke();
        Destroy(gameObject);
    }
}