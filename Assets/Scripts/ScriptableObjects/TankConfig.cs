using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Tank Config")]
public class TankConfig : ScriptableObject
{
    public string tankId;
    public string tankName;

    [Header("Stats")]
    public float baseMaxHP = 100f;
    public float moveSpeed = 6f;
    public float turnSpeed = 120f;

    [Header("Visual")]
    public GameObject tankModelPrefab; // 这个坦克外观Prefab(optional)

    [Header("Loadout")]
    public WeaponConfig defaultWeapon;
}