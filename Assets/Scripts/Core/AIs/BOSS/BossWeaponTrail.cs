using UnityEngine;
using UnityEngine.Serialization;

public class BossWeaponTrailController : MonoBehaviour
{
    public enum TrailSet
    {
        Normal,
        MeleeSkill1,
        MeleeSkill2,
        MeleeSkill3,
        Ranged
    }

    [Header("Trail Sets")]
    [FormerlySerializedAs("trails")]
    [SerializeField] private TrailRenderer[] normalAttackTrails;
    [SerializeField] private TrailRenderer[] meleeSkill1Trails;
    [SerializeField] private TrailRenderer[] meleeSkill2Trails;
    [SerializeField] private TrailRenderer[] meleeSkill3Trails;
    [SerializeField] private TrailRenderer[] rangedAttackTrails;

    private TrailSet activeTrailSet = TrailSet.Normal;

    private void Awake()
    {
        SetAllTrailState(false, clearTrails: true);
    }

    public void SetTrailSet(TrailSet trailSet)
    {
        activeTrailSet = trailSet;
    }

    public void TrailOn()
    {
        SetTrailState(GetTrailsForActiveSet(), true, clearTrails: true);
    }

    public void TrailOff()
    {
        SetAllTrailState(false, clearTrails: false);
    }

    TrailRenderer[] GetTrailsForActiveSet()
    {
        switch (activeTrailSet)
        {
            case TrailSet.MeleeSkill1:
                if (meleeSkill1Trails != null && meleeSkill1Trails.Length > 0)
                    return meleeSkill1Trails;
                break;

            case TrailSet.MeleeSkill2:
                if (meleeSkill2Trails != null && meleeSkill2Trails.Length > 0)
                    return meleeSkill2Trails;
                break;

            case TrailSet.MeleeSkill3:
                if (meleeSkill3Trails != null && meleeSkill3Trails.Length > 0)
                    return meleeSkill3Trails;
                break;

            case TrailSet.Ranged:
                if (rangedAttackTrails != null && rangedAttackTrails.Length > 0)
                    return rangedAttackTrails;
                break;
        }

        return normalAttackTrails;
    }

    void SetAllTrailState(bool enabled, bool clearTrails)
    {
        SetTrailState(normalAttackTrails, enabled, clearTrails);
        SetTrailState(meleeSkill1Trails, enabled, clearTrails);
        SetTrailState(meleeSkill2Trails, enabled, clearTrails);
        SetTrailState(meleeSkill3Trails, enabled, clearTrails);
        SetTrailState(rangedAttackTrails, enabled, clearTrails);
    }

    void SetTrailState(TrailRenderer[] trails, bool enabled, bool clearTrails)
    {
        if (trails == null)
            return;

        foreach (var trail in trails)
        {
            if (trail == null) continue;

            if (clearTrails)
                trail.Clear();

            trail.emitting = false;
            trail.emitting = enabled;
        }
    }
}
