using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankLoadoutApplier : MonoBehaviour
{
    public GameDatabase db;

    [Header("Selected IDs")]
    public string selectedTankId = "tank_basic";
    public string selectedWeaponId = ""; // 空=用坦克默认武器

    [Header("Refs")]
    public PlayerStatsRuntime stats;
    public WeaponController weapon;
    public Transform modelRoot; // 可选：换模型挂这里

    GameObject currentModel;

    void Awake()
    {
        if (stats == null) stats = GetComponent<PlayerStatsRuntime>();
        if (weapon == null) weapon = GetComponent<WeaponController>();
    }

    void Start()
    {
        ApplySelected();
    }

    public void ApplySelected()
    {
        if (db == null) { Debug.LogError("TankLoadoutApplier: db is null"); return; }

        var tank = db.GetTank(selectedTankId);
        if (tank == null) { Debug.LogError("Tank not found: " + selectedTankId); return; }

        // 1) 属性
        stats.ApplyTankConfig(tank, refillHP: true);

        // 2) 模型（可选）
        if (modelRoot != null && tank.tankModelPrefab != null)
        {
            if (currentModel != null) Destroy(currentModel);
            currentModel = Instantiate(tank.tankModelPrefab, modelRoot);
            currentModel.transform.localPosition = Vector3.zero;
            currentModel.transform.localRotation = Quaternion.identity;
        }

        // 3) 武器
        WeaponConfig w = string.IsNullOrEmpty(selectedWeaponId)
            ? tank.defaultWeapon
            : db.GetWeapon(selectedWeaponId);

        weapon.Equip(w);
    }
}