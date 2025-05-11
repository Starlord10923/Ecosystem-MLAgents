using System.Collections.Generic;
using MEC;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    public enum SpawnType { Food, Water, Prey, Predator }

    [Header("Prefabs")]
    public List<GameObject> preyPrefabs;
    public List<GameObject> predatorPrefabs;
    public List<GameObject> foodPrefabs;
    public GameObject waterPrefab;

    [Header("Environment Bounds")]
    public float spawnRadius = 20f;

    [Header("Spawn Settings")]
    public int initialPrey = 10;
    public int initialPredators = 5;
    public int initialFood = 20;
    public int initialWater = 10;

    public bool spawnPrey = true;
    public bool spawnPredators = false;

    [Header("Respawn Settings")]
    public float foodSpawnInterval = 10f;
    public int foodSpawnAmount = 5;
    public float waterSpawnInterval = 20f;
    public int waterSpawnAmount = 2;

    [Header("Collision Avoidance")]
    public float checkRadius = 1.5f;
    public int maxSpawnAttempts = 10;
    public LayerMask obstacleMask;

    GameObject PreyParent;
    GameObject PredatorParent;
    GameObject FoodParent;
    GameObject WaterParent;

    void Start()
    {
        PreyParent = new GameObject($"Spawned-Prey");
        PredatorParent = new GameObject($"Spawned-Predator");
        FoodParent = new GameObject($"Spawned-Food");
        WaterParent = new GameObject($"Spawned-Water");

        if (spawnPrey)
            Spawn(preyPrefabs, initialPrey, SpawnType.Prey);
        if (spawnPredators)
            Spawn(predatorPrefabs, initialPredators, SpawnType.Predator);

        Spawn(foodPrefabs, initialFood, SpawnType.Food);
        Spawn(waterPrefab, initialWater, SpawnType.Water);

        StartCoroutine(SpawnFoodRoutine());
        StartCoroutine(SpawnWaterRoutine());
    }

    void Spawn(List<GameObject> prefabs, int count, SpawnType type)
    {
        if (prefabs.Count == 0 || count <= 0)
        {
            Debug.LogWarning("No prefabs to spawn.");
            return;
        }

        Transform parent = GetParent(type);

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = GetValidSpawnPosition();
            if (pos == Vector3.positiveInfinity) continue;
            int index = Random.Range(0, prefabs.Count);
            Instantiate(prefabs[index], pos, Quaternion.identity, parent);
        }
    }

    void Spawn(GameObject prefab, int count, SpawnType type)
    {
        if (prefab == null || count <= 0)
        {
            Debug.LogWarning("No prefab to spawn.");
            return;
        }

        Transform parent = GetParent(type);

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = GetValidSpawnPosition();
            if (pos == Vector3.positiveInfinity) continue;
            Instantiate(prefab, pos, Quaternion.identity, parent);
        }
    }

    private IEnumerator<float> SpawnFoodRoutine()
    {
        while (true)
        {
            yield return Timing.WaitForSeconds(foodSpawnInterval);
            Spawn(foodPrefabs, foodSpawnAmount, SpawnType.Food);
        }
    }

    private IEnumerator<float> SpawnWaterRoutine()
    {
        while (true)
        {
            yield return Timing.WaitForSeconds(waterSpawnInterval);
            Spawn(waterPrefab, waterSpawnAmount, SpawnType.Water);
        }
    }

    Transform GetParent(SpawnType type)
    {
        return type switch
        {
            SpawnType.Food => FoodParent.transform,
            SpawnType.Water => WaterParent.transform,
            SpawnType.Prey => PreyParent.transform,
            SpawnType.Predator => PredatorParent.transform,
            _ => new GameObject("ErrorSpawn").transform,
        };
    }

    private Vector3 GetValidSpawnPosition()
    {
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            float x = Random.Range(-spawnRadius, spawnRadius);
            float z = Random.Range(-spawnRadius, spawnRadius);
            Vector3 pos = new Vector3(x, 1f, z);

            if (!Physics.CheckSphere(pos, checkRadius, obstacleMask))
                return pos;
        }

        Debug.LogWarning("Could not find a valid spawn position.");
        return Vector3.positiveInfinity; // Signal failure
    }
}
