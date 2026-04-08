using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/Game Database")]
public class GameDatabase : ScriptableObject
{
    public List<TankConfig> tanks = new();
    public List<WeaponConfig> weapons = new();
    public List<EnemyConfig> enemies = new();
    public TankConfig GetTank(string id) =>
        tanks.Find(t => t != null && t.tankId == id);

    public WeaponConfig GetWeapon(string id) =>
        weapons.Find(w => w != null && w.weaponId == id);

    
    public EnemyConfig GetEnemy(string id) =>
        enemies.Find(e => e != null && e.enemyId == id);
}