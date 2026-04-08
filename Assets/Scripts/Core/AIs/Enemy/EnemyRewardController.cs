using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRewardGiver : MonoBehaviour
{
    public EnemyRuntime enemy;
    public PlayerStatsRuntime playerStats; // Stats

    void Awake()
    {
        if (enemy == null) enemy = GetComponent<EnemyRuntime>();
        if (playerStats == null) playerStats = FindFirstObjectByType<PlayerStatsRuntime>();
    }

    void OnEnable()
    {
        if (enemy != null) enemy.OnDied += GiveReward;
    }

    void OnDisable()
    {
        if (enemy != null) enemy.OnDied -= GiveReward;
    }

    void GiveReward()
    {
        if (enemy == null || enemy.config == null) return;
        if (playerStats == null) return;

        playerStats.AddExp(enemy.config.expReward);
        Debug.Log($"Enemy died => +{enemy.config.expReward} EXP");
    }
}