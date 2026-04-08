using UnityEngine;

[CreateAssetMenu(menuName = "Config/Enemy Spawner Config")]
public class EnemySpawnerConfig : ScriptableObject
{
    public string spawnerId;
    public string spawnerName;

    [Header("Spawn Settings")]
    public bool spawnOnStart = true;
    public float spawnInterval = 5f;
    public int maxAliveEnemies = 5;

    [Header("Spawn Position")]
    public bool useRandomRadius = false;
    public float spawnRadius = 5f;

    [Header("Enemy Prefabs")]
    public GameObject[] enemyPrefabs;
}