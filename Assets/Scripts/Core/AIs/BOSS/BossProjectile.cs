using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BossProjectile : MonoBehaviour
{
    public GameObject owner;
    public float damage = 25f;
    public float speed = 12f;
    public float lifeTime = 5f;

    private Vector3 moveDirection;

    public void Initialize(GameObject projectileOwner, Vector3 direction, float projectileDamage, float projectileSpeed, float projectileLifeTime)
    {
        owner = projectileOwner;
        moveDirection = direction.normalized;
        damage = projectileDamage;
        speed = projectileSpeed;
        lifeTime = projectileLifeTime;

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
            playerStats.TakeDamage(damage, owner);
            Destroy(gameObject);
            return;
        }

        if (!other.isStatic)
        {
            Destroy(gameObject);
        }
    }
}