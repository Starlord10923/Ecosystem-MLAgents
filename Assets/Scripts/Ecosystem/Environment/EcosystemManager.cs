using UnityEngine;
using System.Collections.Generic;

public class EcosystemManager : Singleton<EcosystemManager>
{
    public GameObject preyPrefab, predatorPrefab;
    public Vector2 bounds = new(20, 20);

    public Vector3 GetSpawnPosition()
    {
        return new Vector3(Random.Range(-bounds.x, bounds.x), 0.5f, Random.Range(-bounds.y, bounds.y));
    }

    public void Despawn(GameObject agent) => Destroy(agent);
}
