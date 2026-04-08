using System.Collections.Generic;
using UnityEngine;

public class BossMeleeDamageWindow : MonoBehaviour
{
    [System.Serializable]
    public class MeleeDamageConfig
    {
        public int attackIndex;
        public float damage = 20f;
        public float radius = 2f;
        public float facingAngle = 80f;
    }

    [Header("Refs")]
    public BOSSAI ownerAI;
    public Transform hitOrigin;

    [Header("Target")]
    public LayerMask targetLayers = ~0;

    [Header("Configs")]
    public MeleeDamageConfig[] meleeConfigs;

    [Header("Fallback")]
    public float fallbackRadius = 2f;
    public float fallbackDamage = 20f;
    public float fallbackFacingAngle = 80f;

    [Header("Debug")]
    public bool drawGizmos = true;
    public bool drawEvenWhenClosed = true;

    bool windowOpen = false;
    int activeAttackIndex = -1;

    readonly HashSet<GameObject> hitTargetsThisWindow = new HashSet<GameObject>();
    Collider[] cachedTargetColliders;

    void Reset()
    {
        hitOrigin = transform;
    }

    void Update()
    {
        if (!windowOpen)
            return;

        if (ownerAI == null || ownerAI.player == null)
            return;

        Transform origin = hitOrigin != null ? hitOrigin : transform;
        Vector3 center = origin.position;

        float radius = GetActiveRadius();
        float damage = GetActiveDamage();
        float facingAngle = GetActiveFacingAngle();

        if (!IsFacingPlayer(center, facingAngle))
            return;

        CacheTargetCollidersIfNeeded();

        if (cachedTargetColliders == null || cachedTargetColliders.Length == 0)
            return;

        bool hitPlayer = IsAnyTargetColliderInsideSphere(center, radius);
        if (!hitPlayer)
            return;

        GameObject playerRoot = ownerAI.player.root.gameObject;
        if (hitTargetsThisWindow.Contains(playerRoot))
            return;

        ApplyDamageToPlayer(damage);
        hitTargetsThisWindow.Add(playerRoot);
    }

    public void OpenWindow(int attackIndex)
    {
        activeAttackIndex = attackIndex;
        windowOpen = true;
        hitTargetsThisWindow.Clear();
        CacheTargetColliders(forceRefresh: true);
    }

    public void CloseWindow()
    {
        windowOpen = false;
        activeAttackIndex = -1;
        hitTargetsThisWindow.Clear();
    }

    public void ForceCloseWindow()
    {
        CloseWindow();
    }

    void CacheTargetCollidersIfNeeded()
    {
        if (cachedTargetColliders == null || cachedTargetColliders.Length == 0)
            CacheTargetColliders(forceRefresh: true);
    }

    void CacheTargetColliders(bool forceRefresh)
    {
        if (!forceRefresh && cachedTargetColliders != null && cachedTargetColliders.Length > 0)
            return;

        if (ownerAI == null || ownerAI.player == null)
        {
            cachedTargetColliders = System.Array.Empty<Collider>();
            return;
        }

        Transform target = ownerAI.player;

        Collider[] selfAndChildren = target.GetComponentsInChildren<Collider>(true);
        Collider[] parents = target.GetComponentsInParent<Collider>(true);

        int total = (selfAndChildren?.Length ?? 0) + (parents?.Length ?? 0);
        if (total == 0)
        {
            cachedTargetColliders = System.Array.Empty<Collider>();
            return;
        }

        List<Collider> merged = new List<Collider>(total);
        HashSet<Collider> unique = new HashSet<Collider>();

        if (selfAndChildren != null)
        {
            foreach (Collider c in selfAndChildren)
            {
                if (c == null) continue;
                if (!unique.Add(c)) continue;
                if (!IsColliderLayerAllowed(c.gameObject.layer)) continue;
                merged.Add(c);
            }
        }

        if (parents != null)
        {
            foreach (Collider c in parents)
            {
                if (c == null) continue;
                if (!unique.Add(c)) continue;
                if (!IsColliderLayerAllowed(c.gameObject.layer)) continue;
                merged.Add(c);
            }
        }

        cachedTargetColliders = merged.ToArray();
    }

    bool IsColliderLayerAllowed(int layer)
    {
        return (targetLayers.value & (1 << layer)) != 0;
    }

    bool IsAnyTargetColliderInsideSphere(Vector3 center, float radius)
    {
        float radiusSqr = radius * radius;

        foreach (Collider col in cachedTargetColliders)
        {
            if (col == null || !col.enabled)
                continue;

            Vector3 closest = col.ClosestPoint(center);
            float sqr = (closest - center).sqrMagnitude;
            if (sqr <= radiusSqr)
                return true;
        }

        return false;
    }

    bool IsFacingPlayer(Vector3 fromPoint, float maxAngle)
    {
        if (ownerAI == null || ownerAI.player == null)
            return false;

        Vector3 toPlayer = GetPlayerAimPoint() - fromPoint;
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude < 0.001f)
            return true;

        Vector3 forward = ownerAI.transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            return true;

        float angle = Vector3.Angle(forward.normalized, toPlayer.normalized);
        return angle <= maxAngle;
    }

    Vector3 GetPlayerAimPoint()
    {
        if (ownerAI == null || ownerAI.player == null)
            return transform.position;

        if (cachedTargetColliders == null || cachedTargetColliders.Length == 0)
            return ownerAI.player.position;

        Vector3 from = transform.position;
        float bestSqr = float.MaxValue;
        Vector3 bestPoint = ownerAI.player.position;

        foreach (Collider col in cachedTargetColliders)
        {
            if (col == null || !col.enabled)
                continue;

            Vector3 p = col.ClosestPoint(from);
            float sqr = (p - from).sqrMagnitude;

            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                bestPoint = p;
            }
        }

        return bestPoint;
    }

    MeleeDamageConfig GetConfig(int attackIndex)
    {
        if (meleeConfigs == null)
            return null;

        for (int i = 0; i < meleeConfigs.Length; i++)
        {
            if (meleeConfigs[i] != null && meleeConfigs[i].attackIndex == attackIndex)
                return meleeConfigs[i];
        }

        return null;
    }

    float GetActiveRadius()
    {
        MeleeDamageConfig cfg = GetConfig(activeAttackIndex);
        if (cfg != null)
            return Mathf.Max(0f, cfg.radius);

        return Mathf.Max(0f, fallbackRadius);
    }

    float GetActiveDamage()
    {
        MeleeDamageConfig cfg = GetConfig(activeAttackIndex);
        if (cfg != null)
            return Mathf.Max(0f, cfg.damage);

        return Mathf.Max(0f, fallbackDamage);
    }

    float GetActiveFacingAngle()
    {
        MeleeDamageConfig cfg = GetConfig(activeAttackIndex);
        if (cfg != null)
            return Mathf.Clamp(cfg.facingAngle, 0f, 180f);

        return Mathf.Clamp(fallbackFacingAngle, 0f, 180f);
    }

    void ApplyDamageToPlayer(float damage)
    {
        if (ownerAI == null || ownerAI.player == null)
            return;

        Transform target = ownerAI.player;

        MonoBehaviour[] components = target.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour c in components)
        {
            if (c is IDamageable damageable)
            {
                damageable.TakeDamage(damage, ownerAI.gameObject);
                return;
            }
        }

        MonoBehaviour[] childComponents = target.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (MonoBehaviour c in childComponents)
        {
            if (c is IDamageable damageable)
            {
                damageable.TakeDamage(damage, ownerAI.gameObject);
                return;
            }
        }

        MonoBehaviour[] parentComponents = target.GetComponentsInParent<MonoBehaviour>(true);
        foreach (MonoBehaviour c in parentComponents)
        {
            if (c is IDamageable damageable)
            {
                damageable.TakeDamage(damage, ownerAI.gameObject);
                return;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        if (!drawEvenWhenClosed && !windowOpen)
            return;

        Transform origin = hitOrigin != null ? hitOrigin : transform;
        Vector3 center = origin.position;

        float radius = windowOpen ? GetActiveRadius() : Mathf.Max(0f, fallbackRadius);

        Gizmos.color = windowOpen ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(center, radius);
    }
}