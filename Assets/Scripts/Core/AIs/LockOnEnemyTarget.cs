using System;
using UnityEngine;

public class LockOnTarget : MonoBehaviour
{
    [Header("Lock-On")]
    public Transform headPoint;
    public bool isDead = false;

    public event Action<LockOnTarget> OnLockTargetDied;

    void Awake()
    {
        // ∑¿÷π–Ú¡–ªØ≤–¡Ù
        isDead = false;

        if (headPoint == null)
        {
            GameObject autoHead = new GameObject("AutoHeadPoint");
            autoHead.transform.SetParent(transform);
            autoHead.transform.localPosition = new Vector3(0f, 2f, 0f);
            headPoint = autoHead.transform;
        }
    }

    public void NotifyDied()
    {
        if (isDead) return;

        isDead = true;
        OnLockTargetDied?.Invoke(this);
    }

    public bool CanBeLocked()
    {
        return !isDead && gameObject.activeInHierarchy;
    }

    public Transform GetHeadPoint()
    {
        return headPoint != null ? headPoint : transform;
    }
}