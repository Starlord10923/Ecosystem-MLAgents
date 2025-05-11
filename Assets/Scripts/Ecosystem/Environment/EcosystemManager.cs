using UnityEngine;

public class EcosystemManager : Singleton<EcosystemManager>
{
    public GameObject preyPrefab, predatorPrefab;
    public Vector2 bounds = new(20, 20);

    public Vector3 GetSpawnPosition()
    {
        return new Vector3(Random.Range(-bounds.x, bounds.x), 0.5f, Random.Range(-bounds.y, bounds.y));
    }

    public void SpawnAnimal(AgentAnimalBase parent, AgentStats childStats, Vector3 spawnPos)
    {
        GameObject animalPrefab = (parent is PreyAgent) ? preyPrefab : predatorPrefab;

        GameObject childObj = Instantiate(animalPrefab, spawnPos, Quaternion.identity);
        if (childObj.TryGetComponent(out AgentBase agentBase))
        {
            var clonedStats = AgentStats.Clone(childStats);
            agentBase.InitializeStats(clonedStats);
        }
    }

    public void Remove(GameObject agent) => Destroy(agent);
}
