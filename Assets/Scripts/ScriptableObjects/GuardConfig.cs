using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Guard Config")]
public class GuardConfig : ScriptableObject
{
    public string guardId;
    public string guardName;

    [Header("Core Data")]
    public int level = 1;
    public float maxHP = 50f;

    [Header("Affect Value")]
    public float AffectValue = 14f; // affect value£¬ damage/heal/ect.

    public GameObject guardPrefab;
}