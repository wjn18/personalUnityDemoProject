using UnityEngine;

[CreateAssetMenu(menuName = "Config/Base Config")]
public class BaseConfig : ScriptableObject
{
    public string baseId;
    public string baseName;

    [Header("Stats")]
    public int level = 1;
    public float maxHP = 800f;

    [Header("Initial State")]
    public bool takenAtStart = false;

    [Header("Visual")]
    public GameObject normalModel;
    public GameObject brokenModel;
    public GameObject takenModel;
}