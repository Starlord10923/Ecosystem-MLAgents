using Unity.MLAgents.Actuators;
using UnityEngine;

public class PredatorAgent : AgentAnimalBase
{
    public override void OnEpisodeBegin()
    {
        InitializeStats();
        transform.position = EcosystemManager.Instance.GetSpawnPosition();
        rb.velocity = Vector3.zero;
    }

    public override void CollectObservations(Unity.MLAgents.Sensors.VectorSensor sensor)
    {
        sensor.AddObservation(stats.hunger);
        sensor.AddObservation(stats.thirst);
        sensor.AddObservation(stats.CanMate ? 1f : 0f);
        sensor.AddObservation(stats.speed / 5f);
        sensor.AddObservation(stats.size / 2f);
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        currentMove = new Vector2(actions.ContinuousActions[0], actions.ContinuousActions[1]);
        brake = Mathf.Clamp01(actions.DiscreteActions[0]);
    }

    protected override void Die()
    {
        AddReward(-1f);
        EcosystemManager.Instance.Despawn(gameObject);
        EndEpisode();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if (other.TryGetComponent<PreyAgent>(out var prey))
        {
            prey.AddReward(-1f);
            Destroy(prey.gameObject);
            Eat(0.6f);
            AddReward(1.0f);
        }
    }
}
