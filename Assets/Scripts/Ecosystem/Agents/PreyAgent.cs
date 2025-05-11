using Unity.MLAgents.Actuators;
using UnityEngine;

[RequireComponent(typeof(SustainedConsumable))]
public class PreyAgent : AgentAnimalBase
{
    private SustainedConsumable sustainedConsumable;
    protected override void Awake()
    {
        base.Awake();
        sustainedConsumable = GetComponent<SustainedConsumable>();
    }

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

    public override void UpdateSize()
    {
        if (transform.localScale.y != stats.CurrentSize)
        {
            transform.localScale = Vector3.one * stats.CurrentSize;
            sustainedConsumable.UpdateFromSize(stats.CurrentSize);
        }
    }

    public override void Die()
    {
        base.Die();
        EcosystemManager.Instance.Remove(gameObject);
        EndEpisode();
    }
}
