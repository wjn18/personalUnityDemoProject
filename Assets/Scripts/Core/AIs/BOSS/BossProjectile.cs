using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BossProjectile : MonoBehaviour
{
    public GameObject owner;
    public float damage = 30f;
    public float speed = 30f;
    public float lifeTime = 5f;

    private Vector3 moveDirection;
    private BOSSAI ownerAI;
    private bool resolvedImpact;

    public void Initialize(GameObject projectileOwner, BOSSAI projectileOwnerAI, Vector3 direction, float projectileDamage, float projectileSpeed, float projectileLifeTime)
    {
        owner = projectileOwner;
        ownerAI = projectileOwnerAI;
        moveDirection = direction.normalized;
        damage = projectileDamage;
        speed = projectileSpeed;
        lifeTime = projectileLifeTime;
        resolvedImpact = false;

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        TryHit(other.gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        TryHit(collision.gameObject);
    }

    void TryHit(GameObject other)
    {
        if (other == null)
            return;

        if (owner != null && (other == owner || other.transform.root.gameObject == owner))
            return;

        PlayerStatsRuntime playerStats = other.GetComponentInParent<PlayerStatsRuntime>();
        if (playerStats != null)
        {
            Collider hitCollider = other.GetComponent<Collider>();
            if (hitCollider == null)
                hitCollider = other.GetComponentInParent<Collider>();

            Vector3 hitPoint = hitCollider != null
                ? hitCollider.ClosestPoint(transform.position)
                : other.transform.position;

            if (hitCollider != null && (hitPoint - transform.position).sqrMagnitude < 0.0001f)
                hitPoint = hitCollider.bounds.center;

            Vector3 hitNormal = hitPoint - transform.position;
            if (hitNormal.sqrMagnitude < 0.0001f)
                hitNormal = -moveDirection;

            playerStats.TakeDamage(damage, owner);
            resolvedImpact = true;

            if (ownerAI != null && ownerAI.combatAudioController != null)
                ownerAI.combatAudioController.PlayHitEffect(hitPoint, hitNormal);

            if (ScreenShakeController.Instance != null)
                ScreenShakeController.Instance.PlayBossRangedHitShake();

            if (ownerAI != null)
                ownerAI.NotifyAttackHitConfirmed();

            Destroy(gameObject);
            return;
        }

        if (!other.isStatic)
        {
            resolvedImpact = true;

            if (ownerAI != null)
                ownerAI.NotifyRangedAttackMiss();

            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (resolvedImpact)
            return;

        if (ownerAI != null)
            ownerAI.NotifyRangedAttackMiss();
    }
}
