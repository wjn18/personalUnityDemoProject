using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

public class WeaponController : MonoBehaviour
{
    public Transform FirePoint;
    WeaponConfig current;
    float nextFireTime;

    void Awake()
    {
        if (FirePoint == null)
            FirePoint = GameObject.Find("FirePoint").transform;
    }
    public void Equip(WeaponConfig cfg)
    {
        current = cfg;
        nextFireTime = 0f;
    }

    public void TryFire()
    {
        if (current == null) return;
        if (FirePoint == null) return;
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + current.fireCooldown;

        var shellObj = Instantiate(
            current.shellPrefab,
            FirePoint.position,
            FirePoint.rotation
        );

        var proj = shellObj.GetComponent<Projectile>();
        if (proj != null) proj.damage = current.damage;

        var rb = shellObj.GetComponent<Rigidbody>();
        if (rb != null)
            rb.AddForce(FirePoint.forward * current.launchForce, ForceMode.Impulse);
    }
}
