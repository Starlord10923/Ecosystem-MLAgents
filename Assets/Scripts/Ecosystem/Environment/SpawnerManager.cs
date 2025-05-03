using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
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

    void Start()
    {
        if (spawnPrey)
            Spawn(preyPrefabs, initialPrey, "Prey");
        if (spawnPredators)
            Spawn(predatorPrefabs, initialPredators, "Predators");
        Spawn(foodPrefabs, initialFood, "Food");
        Spawn(waterPrefab, initialWater, "Water");
    }

    void Spawn(List<GameObject> prefabs, int count, string type)
    {
        if (prefabs.Count == 0 || count <= 0)
        {
            Debug.LogWarning("No prefabs to spawn.");
            return;
        }

        Transform parent = new GameObject($"Spawned-{type}").transform;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = RandomSpawnPosition();
            int index = Random.Range(0, prefabs.Count);
            Instantiate(prefabs[index], pos, Quaternion.identity, parent);
        }
    }

    void Spawn(GameObject prefab, int count, string type)
    {
        if (prefab == null || count <= 0)
        {
            Debug.LogWarning("No prefab to spawn.");
            return;
        }

        Transform parent = new GameObject($"Spawned-{type}").transform;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = RandomSpawnPosition();
            Instantiate(prefab, pos, Quaternion.identity, parent);
        }
    }

    Vector3 RandomSpawnPosition()
    {
        float x = Random.Range(-spawnRadius, spawnRadius);
        float z = Random.Range(-spawnRadius, spawnRadius);
        return new Vector3(x, 0.5f, z);
    }
}
