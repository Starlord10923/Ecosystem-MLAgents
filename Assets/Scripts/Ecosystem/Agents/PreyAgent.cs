using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

[RequireComponent(typeof(SustainedConsumable))]
public class PreyAgent : AgentAnimalBase
{
    private SustainedConsumable sustainedConsumable;
    protected override void Awake()
    {
        base.Awake();
        sustainedConsumable = GetComponent<SustainedConsumable>();
        animalType = AnimalType.Prey;
    }

    public override void OnEpisodeBegin()
    {
        InitializeStats();
        rb.velocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Vital stats
        sensor.AddObservation(stats.hunger);
        sensor.AddObservation(stats.thirst);
        sensor.AddObservation(stats.health);

        sensor.AddObservation(stats.hunger / stats.hungerDecayRate); // seconds to starvation
        sensor.AddObservation(stats.thirst / stats.thirstDecayRate); // seconds to dehydration

        // Perception & physical traits
        sensor.AddObservation(stats.sightRange);
        sensor.AddObservation(stats.CurrentSize / 3f);
        sensor.AddObservation(stats.maxSize / 3f);
        sensor.AddObservation(stats.speed / 5f);

        // Lifecycle & reproduction
        sensor.AddObservation(stats.CanMate ? 1f : 0f);
        sensor.AddObservation(Mathf.Clamp01(stats.age / stats.growthTime));     // growth progress
        sensor.AddObservation(stats.age / stats.MaxLifetime);                   // life progress
        sensor.AddObservation(Mathf.Clamp01(stats.numChildren / 10f));          // normalized offspring count

        // Motion & orientation
        sensor.AddObservation(rb.velocity.x / maxSpeed);
        sensor.AddObservation(rb.velocity.z / maxSpeed);
        sensor.AddObservation(transform.forward.x);
        sensor.AddObservation(transform.forward.z);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        currentMove = new Vector2(actions.ContinuousActions[0], actions.ContinuousActions[1]);
        brake = Mathf.Clamp01(actions.DiscreteActions[0]);
        PenalizeCrowding();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuous = actionsOut.ContinuousActions;
        var discrete = actionsOut.DiscreteActions;

        continuous[0] = Input.GetAxis("Horizontal");
        continuous[1] = Input.GetAxis("Vertical");

        discrete[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;

        CustomLogger.Log($"Heuristic called : {continuous[0]},{continuous[1]},{discrete[0]}");
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
