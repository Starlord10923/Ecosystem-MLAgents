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
        sensor.AddObservation(stats.CurrentSize / 3f);
        sensor.AddObservation(stats.maxSize / 3f);
        sensor.AddObservation(stats.speed / 5f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        currentMove = new Vector2(actions.ContinuousActions[0], actions.ContinuousActions[1]);
        brake = Mathf.Clamp01(actions.DiscreteActions[0]);
    }

    public override void Die()
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
            if (prey.stats.CurrentSize <= stats.CurrentSize)
            {
                AddReward(1f);
                prey.AddReward(-1f);
                prey.Die();
                stats.Eat(0.6f);
            }
        }
    }
}
