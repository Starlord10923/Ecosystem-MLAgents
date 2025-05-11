using System.Collections.Generic;
using MEC;
using UnityEngine;

public class SpawnerManager : Singleton<SpawnerManager>
{
    public enum SpawnType { Food, Water, Prey, Predator }

    [Header("Prefabs")]
    public List<GameObject> preyPrefabs;
    public List<GameObject> predatorPrefabs;
    public List<GameObject> foodPrefabs;
    public GameObject waterPrefab;

    [Header("Spawn Settings")]
    public int initialPrey = 10;
    public int initialPredators = 5;
    public int initialFood = 20;
    public int initialWater = 10;

    public bool spawnPrey = true;
    public bool spawnPredators = false;

    public bool spawnInQuadrants = true;

    [Header("Respawn Settings")]
    public bool SpawnInIntervals = true;
    public float foodSpawnInterval = 10f;
    public int foodSpawnAmount = 5;
    public float waterSpawnInterval = 20f;
    public int waterSpawnAmount = 2;

    [Header("Collision Avoidance")]
    public float checkRadius = 1.15f;
    public int maxSpawnAttempts = 10;
    public LayerMask obstacleMask;
    public TagMask blockingTags;

    GameObject PreyParent;
    GameObject PredatorParent;
    GameObject FoodParent;
    GameObject WaterParent;

    CoroutineHandle foodSpawnHandle;
    CoroutineHandle waterSpawnHandle;

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

        if (SpawnInIntervals)
        {
            foodSpawnHandle = Timing.RunCoroutine(SpawnFoodRoutine());
            waterSpawnHandle = Timing.RunCoroutine(SpawnWaterRoutine());
        }
    }

    [ContextMenu("Spawn Prey")]
    public void SpawnPrey() => Spawn(preyPrefabs, initialPrey, SpawnType.Prey);

    void Spawn(List<GameObject> prefabs, int count, SpawnType type)
    {
        if (prefabs.Count == 0 || count <= 0) return;

        Transform parent = GetParent(type);
        int quadrant = type switch
        {
            SpawnType.Prey => 1,
            SpawnType.Predator => 3,
            _ => 0    // food / water anywhere
        };

        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, prefabs.Count);
            Vector3 pos = GetValidSpawnPosition(prefabs[idx], quadrant);
            if (pos == Vector3.positiveInfinity) continue;

            Quaternion rot = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            Instantiate(prefabs[idx], pos, rot, parent);
        }
    }

    void Spawn(GameObject prefab, int count, SpawnType type)
    {
        if (prefab == null || count <= 0) return;

        Transform parent = GetParent(type);
        int quadrant = (type == SpawnType.Predator) ? 3 :
                       (type == SpawnType.Prey) ? 1 : 0;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = GetValidSpawnPosition(prefab, quadrant);
            if (pos == Vector3.positiveInfinity) continue;
            Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            Instantiate(prefab, pos, rot, parent);
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

    public void Reinitialise()
    {
        Destroy(PreyParent);
        Destroy(PredatorParent);
        Destroy(FoodParent);
        Destroy(WaterParent);
        Timing.KillCoroutines(foodSpawnHandle);
        Timing.KillCoroutines(waterSpawnHandle);
        Start();               // fresh start
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

    static readonly Collider[] probeHits = new Collider[16];   // tweak size as needed
    Vector3 GetValidSpawnPosition(GameObject prefab, int quadrant = 0)
    {
        if (!spawnInQuadrants) quadrant = 0;
        float radius = checkRadius * Mathf.Max(prefab.transform.localScale.x,
                                               prefab.transform.localScale.y,
                                               prefab.transform.localScale.z);

        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            Vector3 pos = EcosystemManager.Instance.GetSpawnPosition(quadrant);
            pos.y = prefab.transform.localScale.y;

            int hitCount = Physics.OverlapSphereNonAlloc(
                pos, radius, probeHits, obstacleMask, QueryTriggerInteraction.Collide);

            bool blocked = false;
            for (int i = 0; i < hitCount; i++)
            {
                Collider c = probeHits[i];
                if (!c || string.IsNullOrEmpty(c.tag)) continue;
                if (blockingTags.Contains(c.tag)) { blocked = true; break; }
            }

            if (!blocked) return pos;
        }

        CustomLogger.LogWarning($"[SpawnerManager] Could not place {prefab.name} in quadrant {quadrant}");
        return Vector3.positiveInfinity;
    }
}
