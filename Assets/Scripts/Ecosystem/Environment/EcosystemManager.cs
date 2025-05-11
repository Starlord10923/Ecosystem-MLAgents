using System.Collections.Generic;
using UnityEngine;

public class EcosystemManager : Singleton<EcosystemManager>
{
    public GameObject Environment;
    public Vector2 bounds = new(20, 20);

    /* ─── Active-agent tracking ─── */
    private readonly HashSet<AgentBase> liveAgents = new();

    public void Register(AgentBase agent)
    {
        liveAgents.Add(agent);
    }
    public void Unregister(AgentBase agent)
    {
        liveAgents.Remove(agent);

        // if everyone died, restart environment & episode
        if (liveAgents.Count == 0)
        {
            ResetEnvironment();
        }
    }

    private void FixedUpdate()
    {
        // 1) record current population size
        Stats.RecordSurvival(liveAgents.Count);

        float ageSum = 0f;
        foreach (var agent in liveAgents)
            ageSum += agent.stats.age;

        Stats.RecordMeanAge(ageSum / liveAgents.Count);
    }

    public Vector3 GetSpawnPosition()
    {
        return Environment.transform.position + new Vector3(Random.Range(-bounds.x, bounds.x), 1f, Random.Range(-bounds.y, bounds.y));
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
        // Destroy every resource & animal still present
        Telemetry.Instance.OnEpisodeEnd();
        foreach (var animal in FindObjectsByType<AgentAnimalBase>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID))
        {
            Destroy(animal.gameObject);
        }
        foreach (var consumable in FindObjectsByType<SustainedConsumable>(FindObjectsInactive.Include, FindObjectsSortMode.InstanceID))
        {
            Destroy(consumable.gameObject);
        }

        // Let a SpawnerManager rebuild the scene
        SpawnerManager.Instance.Reinitialise();

        liveAgents.Clear();
        Telemetry.Instance.OnEpisodeBegin();   // after re-spawn
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
    }
#endif
}
