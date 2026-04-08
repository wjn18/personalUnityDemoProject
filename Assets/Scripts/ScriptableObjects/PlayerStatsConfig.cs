using UnityEngine;

[CreateAssetMenu(menuName = "Config/Player Stats Config")]
public class PlayerStatsConfig : ScriptableObject
{
    [Header("Health")]
    public float baseMaxHP = 100f;
    public float hpPerLevel = 10f;

    [Header("Stamina / SP")]
    public float baseMaxSP = 100f;
    public float spPerLevel = 5f;

    [Header("EXP")]
    public int baseExpToNext = 100;
    public float expGrowthPow = 1.2f;

    public float GetMaxHP(int level)
    {
        level = Mathf.Max(1, level);
        return baseMaxHP + (level - 1) * hpPerLevel;
    }

    public float GetMaxSP(int level)
    {
        level = Mathf.Max(1, level);
        return baseMaxSP + (level - 1) * spPerLevel;
    }

    public int GetExpToNext(int level)
    {
        level = Mathf.Max(1, level);
        return Mathf.RoundToInt(baseExpToNext * Mathf.Pow(level, expGrowthPow));
    }
}