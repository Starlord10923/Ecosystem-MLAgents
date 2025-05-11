using System.Collections.Generic;
using UnityEngine;

public class EcosystemManager : Singleton<EcosystemManager>
{
    public GameObject Environment;
    public Vector2 bounds = new(20, 20);
    public bool UseHeuristicControl = false;  // Toggle globally in Inspector or via UI
    public bool showValues = false;

    /* ─── Active-agent tracking ─── */
    private readonly HashSet<AgentBase> liveAgents = new();
    private bool isResettingEnvironment = false;

    // EcosystemManager.cs (inside EcosystemManager class)

    [Header("Ecosystem Metrics")]
    public int totalEpisodes = 0;
    public float totalRewardGiven = 0f;
    public float totalPenaltyGiven = 0f;
    public float crowdingPenalty = 0f;
    public int totalPreySpawned = 0;
    public int totalPredatorsSpawned = 0;
    public int currentPreyCount = 0;
    public int currentPredatorCount = 0;
    public int foodConsumed = 0;
    public int waterConsumed = 0;
    public int totalMating = 0;
    public float partialMatingReward = 0f;
    public int animalKilled = 0;
    public int reachedLifeEnd = 0;
    public int diedFromHunger = 0;
    public int diedFromThirst = 0;

    public void Register(AgentBase agent)
    {
        liveAgents.Add(agent);
        if (agent is PreyAgent)
        {
            currentPreyCount += 1;
            totalPreySpawned += 1;
        }
        else
        {
            currentPredatorCount += 1;
            totalPredatorsSpawned += 1;
        }
    }
    public void Unregister(AgentBase agent)
    {
        liveAgents.Remove(agent);
        if (agent is PreyAgent) currentPreyCount -= 1;
        else currentPredatorCount -= 1;

        // if everyone died, restart environment & episode
        if (liveAgents.Count == 0 && !isResettingEnvironment)
        {
            ResetEnvironment();
        }
    }

    private static readonly List<AgentBase> agentBuffer = new(128);
    private void FixedUpdate()
    {
        int count = liveAgents.Count;
        if (count == 0)
            return;

        float ageSum = 0f;

        agentBuffer.Clear();
        agentBuffer.AddRange(liveAgents); // avoids HashSet enumeration allocation

        for (int i = 0; i < agentBuffer.Count; i++)
        {
            ageSum += agentBuffer[i].stats.age;
        }

        Stats.RecordSurvival(count);
        Stats.RecordMeanAge(ageSum / count);
    }

    public Vector3 GetSpawnPosition(int quadrant = 0)
    {
        Vector3 center = Environment.transform.position;   // origin of the whole map

        float halfX = bounds.x;    // positive half-extent in X
        float halfZ = bounds.y;    // positive half-extent in Z
        CustomLogger.Log(quadrant);

        float x = quadrant switch
        {
            1 => Random.Range(0f, halfX),   // +x
            2 => Random.Range(-halfX, 0f),    // –x
            3 => Random.Range(-halfX, 0f),    // –x
            4 => Random.Range(0f, halfX),   // +x
            _ => Random.Range(-halfX, halfX)  // any
        };

        float z = quadrant switch
        {
            1 => Random.Range(0f, halfZ),   // +z
            2 => Random.Range(0f, halfZ),   // +z
            3 => Random.Range(-halfZ, 0f),    // –z
            4 => Random.Range(-halfZ, 0f),    // –z
            _ => Random.Range(-halfZ, halfZ)  // any
        };

        return center + new Vector3(x, 1f, z);
    }

    public void SpawnAnimal(AgentAnimalBase parent, AgentStats childStats, Vector3 spawnPos)
    {
        GameObject animalPrefab = parent is PreyAgent ? SpawnerManager.Instance.preyPrefabs[0] : SpawnerManager.Instance.predatorPrefabs[0];

        GameObject child = Instantiate(animalPrefab, spawnPos, Quaternion.identity);
        if (child.TryGetComponent(out AgentBase agentBase))
        {
            agentBase.InitializeStats(AgentStats.Clone(childStats));   // newborn stats
        }
        Telemetry.Instance.OnAgentSpawn();
    }

    /* ─── world reset ─── */
    public void ResetEnvironment()
    {
        if (isResettingEnvironment || SpawnerManager.Instance == null || Telemetry.Instance == null)
            return;

        isResettingEnvironment = true;
        totalEpisodes += 1;

        Telemetry.Instance.OnEpisodeEnd(this);

        // Cache to avoid modifying collection during iteration
        var allAnimals = new List<AgentAnimalBase>(FindObjectsByType<AgentAnimalBase>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID));
        foreach (var animal in allAnimals)
            Destroy(animal.gameObject); // Triggers OnDestroy later

        var allConsumables = new List<SustainedConsumable>(FindObjectsByType<SustainedConsumable>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID));
        foreach (var consumable in allConsumables)
            Destroy(consumable.gameObject);

        liveAgents.Clear(); // explicit clear, no need to re-unregister on destroy

        SpawnerManager.Instance.Reinitialise();
        Telemetry.Instance.OnEpisodeBegin();

        isResettingEnvironment = false;
    }

    /* convenience remove wrapper */
    public void Remove(GameObject agent) => Destroy(agent);

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = transform.position;
        Vector3 size = new(bounds.x * 2, 1f, bounds.y * 2); // y=1 for a flat cube
        Gizmos.DrawWireCube(center, size);

        // Draw quadrant lines
        Vector3 xStart = center + new Vector3(-bounds.x, 0, 0);
        Vector3 xEnd = center + new Vector3(bounds.x, 0, 0);
        Vector3 zStart = center + new Vector3(0, 0, -bounds.y);
        Vector3 zEnd = center + new Vector3(0, 0, bounds.y);
        Gizmos.DrawLine(xStart, xEnd);
        Gizmos.DrawLine(zStart, zEnd);

        // Define quadrant centers
        Vector3 q1 = center + new Vector3(bounds.x / 2, 0, bounds.y / 2);
        Vector3 q2 = center + new Vector3(-bounds.x / 2, 0, bounds.y / 2);
        Vector3 q3 = center + new Vector3(-bounds.x / 2, 0, -bounds.y / 2);
        Vector3 q4 = center + new Vector3(bounds.x / 2, 0, -bounds.y / 2);

        // Draw labels
        UnityEditor.Handles.color = Color.white;
        var labelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };

        // Draw quadrant labels
        UnityEditor.Handles.Label(q1 + Vector3.up * 2f, "Quadrant 1\nPrey", labelStyle);
        UnityEditor.Handles.Label(q2 + Vector3.up * 2f, "Quadrant 2", labelStyle);
        UnityEditor.Handles.Label(q3 + Vector3.up * 2f, "Quadrant 3\nPredator", labelStyle);
        UnityEditor.Handles.Label(q4 + Vector3.up * 2f, "Quadrant 4", labelStyle);
    }
#endif
}
