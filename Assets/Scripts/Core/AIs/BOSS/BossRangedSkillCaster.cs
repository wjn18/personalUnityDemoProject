using UnityEngine;

public class BossRangedSkillCaster : MonoBehaviour
{
    [System.Serializable]
    public class ProjectileConfig
    {
        public int attackIndex = 6;
        public BossProjectile projectilePrefab;
        public Transform firePoint;
        public float damage = 25f;
        public float speed = 12f;
        public float lifeTime = 5f;
    }

    [Header("Refs")]
    public BOSSAI ownerAI;
    public Transform target;

    [Header("Configs")]
    public ProjectileConfig[] projectileConfigs;

    public void FireAtPlayer(int attackIndex)
    {
        ProjectileConfig cfg = GetConfig(attackIndex);
        if (cfg == null || cfg.projectilePrefab == null || cfg.firePoint == null)
            return;

        Transform targetTransform = target;
        if (targetTransform == null && ownerAI != null)
            targetTransform = ownerAI.player;

        Vector3 direction;
        if (targetTransform != null)
        {
            Vector3 aimPoint = targetTransform.position + Vector3.up * 1f;
            direction = (aimPoint - cfg.firePoint.position).normalized;
        }
        else
        {
            direction = cfg.firePoint.forward;
        }

        BossProjectile projectile = Instantiate(
            cfg.projectilePrefab,
            cfg.firePoint.position,
            Quaternion.LookRotation(direction, Vector3.up)
        );

        projectile.Initialize(
            ownerAI != null ? ownerAI.gameObject : gameObject,
            direction,
            cfg.damage,
            cfg.speed,
            cfg.lifeTime
        );
    }

    ProjectileConfig GetConfig(int attackIndex)
    {
        if (projectileConfigs == null)
            return null;

        for (int i = 0; i < projectileConfigs.Length; i++)
        {
            if (projectileConfigs[i] != null && projectileConfigs[i].attackIndex == attackIndex)
                return projectileConfigs[i];
        }

        return null;
    }
}