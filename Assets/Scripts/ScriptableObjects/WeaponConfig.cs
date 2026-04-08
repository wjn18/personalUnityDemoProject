using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Weapon Config")]
public class WeaponConfig : ScriptableObject
{
    public string weaponId;
    public string weaponName;

    [Header("Damage")]
    public float damage = 30f;

    [Header("Fire")]
    public float fireCooldown = 0.25f;
    public float launchForce = 2000f;

    [Header("Projectile")]
    public GameObject shellPrefab;
}