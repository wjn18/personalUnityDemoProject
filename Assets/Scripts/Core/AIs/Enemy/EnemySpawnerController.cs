using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawnerController : MonoBehaviour
{
    [Header("Config")]
    public EnemySpawnerConfig config;

    [Header("Refs")]
    public BaseRuntime linkedBase;
    public Transform spawnPoint;

    [Header("NavMesh Spawn")]
    public bool snapToNavMesh = true;          // 是否自动吸附到 NavMesh
    public float navMeshSearchRadius = 6f;    // 查找最近 NavMesh 的半径
    public float spawnHeightOffset = 1f;      // 生成后往上抬一点，避免半截埋地

    [Header("Runtime")]
    public int currentAliveEnemies = 0;
    public bool isSpawning = false;

    Coroutine spawnRoutine;
    readonly List<EnemyRuntime> aliveEnemies = new List<EnemyRuntime>();

    void Start()
    {
        if (config == null)
        {
            Debug.LogError($"{name}: EnemySpawner 没有绑定 config!");
            return;
        }

        if (linkedBase != null)
        {
            linkedBase.OnStateChanged += HandleBaseStateChanged;
        }

        if (config.spawnOnStart && CanSpawnNow())
        {
            StartSpawning();
        }
    }

    void OnDestroy()
    {
        if (linkedBase != null)
        {
            linkedBase.OnStateChanged -= HandleBaseStateChanged;
        }
    }

    bool CanSpawnNow()
    {
        if (config == null) return false;
        if (linkedBase == null) return true;

        return linkedBase.state == BaseRuntime.BaseState.NormalEnemy;
    }

    void HandleBaseStateChanged(BaseRuntime.BaseState newState)
    {
        if (newState == BaseRuntime.BaseState.NormalEnemy)
        {
            StartSpawning();
        }
        else
        {
            StopSpawning();
        }
    }

    public void StartSpawning()
    {
        if (config == null) return;
        if (!CanSpawnNow()) return;
        if (spawnRoutine != null) return;

        spawnRoutine = StartCoroutine(SpawnLoop());
        isSpawning = true;
    }

    public void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        isSpawning = false;
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            CleanupDeadReferences();

            if (currentAliveEnemies < config.maxAliveEnemies)
            {
                SpawnEnemy();
            }

            yield return new WaitForSeconds(config.spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        if (config.enemyPrefabs == null || config.enemyPrefabs.Length == 0)
        {
            Debug.LogWarning($"{name}: 没有可生成的 enemy prefab!");
            return;
        }

        GameObject prefab = GetRandomEnemyPrefab();
        if (prefab == null)
        {
            Debug.LogWarning($"{name}: 选中的 enemy prefab 是 null");
            return;
        }

        Vector3 pos = GetSpawnPosition();
        Quaternion rot = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

        GameObject enemyObj = Instantiate(prefab, pos, rot);

        // 自动吸附到 NavMesh
        if (snapToNavMesh)
        {
            SnapObjectToNavMesh(enemyObj);
        }

        EnemyRuntime enemy = enemyObj.GetComponent<EnemyRuntime>();
        if (enemy != null)
        {
            currentAliveEnemies++;
            aliveEnemies.Add(enemy);
            enemy.OnDied += () => HandleEnemyDied(enemy);
        }
        else
        {
            Debug.LogWarning($"{name}: {enemyObj.name} 没有 EnemyRuntime 组件");
        }
    }

    GameObject GetRandomEnemyPrefab()
    {
        int index = Random.Range(0, config.enemyPrefabs.Length);
        return config.enemyPrefabs[index];
    }

    Vector3 GetSpawnPosition()
    {
        Vector3 center = spawnPoint != null ? spawnPoint.position : transform.position;

        if (!config.useRandomRadius)
            return center;

        Vector2 random2D = Random.insideUnitCircle * config.spawnRadius;
        return new Vector3(center.x + random2D.x, center.y, center.z + random2D.y);
    }

    void SnapObjectToNavMesh(GameObject obj)
    {
        if (obj == null) return;

        Vector3 searchOrigin = obj.transform.position + Vector3.up * 2f;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(searchOrigin, out hit, navMeshSearchRadius, NavMesh.AllAreas))
        {
            obj.transform.position = hit.position + Vector3.up * spawnHeightOffset;
        }
        else
        {
            Debug.LogWarning($"{name}: 没找到附近的 NavMesh，敌人生成位置有问题 -> {obj.name}");
        }
    }

    void HandleEnemyDied(EnemyRuntime enemy)
    {
        if (enemy != null)
        {
            aliveEnemies.Remove(enemy);
        }

        currentAliveEnemies = Mathf.Max(0, aliveEnemies.Count);
    }

    void CleanupDeadReferences()
    {
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (aliveEnemies[i] == null)
            {
                aliveEnemies.RemoveAt(i);
            }
        }

        currentAliveEnemies = aliveEnemies.Count;
    }

    // 强制生成一个敌人
    public void ForceSpawnOne()
    {
        CleanupDeadReferences();

        if (currentAliveEnemies < config.maxAliveEnemies && CanSpawnNow())
        {
            SpawnEnemy();
        }
    }

    public void ClearAllSpawnedEnemies()
    {
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (aliveEnemies[i] != null)
            {
                Destroy(aliveEnemies[i].gameObject);
            }
        }

        aliveEnemies.Clear();
        currentAliveEnemies = 0;
    }

    // 显示生成范围
    void OnDrawGizmos()
    {
        if (config == null) return;
        if (!config.useRandomRadius) return;

        Vector3 center = spawnPoint != null ? spawnPoint.position : transform.position;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(center, config.spawnRadius);
    }   
}