using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AgentBase : Agent
{
    protected Rigidbody rb;
    protected Vector2 currentMove;
    protected float brake;
    public AgentStats stats;
    public float maxSpeed;

    [SerializeField] protected AnimalBar animalBar;

    protected override void OnEnable()
    {
        base.OnEnable();
        EcosystemManager.Instance.Register(this);
    }

    private void OnDestroy()
    {
        if (!Application.isPlaying || EcosystemManager.Instance == null)
            return;

        EcosystemManager.Instance.Unregister(this);
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animalBar = GetComponentInChildren<AnimalBar>();
    }

    protected virtual void Start()
    {
        if (TryGetComponent<Unity.MLAgents.Policies.BehaviorParameters>(out var bp))
        {
            bp.BehaviorType = EcosystemManager.Instance.UseHeuristicControl
                ? Unity.MLAgents.Policies.BehaviorType.HeuristicOnly
                : Unity.MLAgents.Policies.BehaviorType.Default;
            if (!EcosystemManager.Instance.UseHeuristicControl)
            {
                if (this is PreyAgent && EcosystemManager.Instance.PreyBrain != null)
                    bp.Model = EcosystemManager.Instance.PreyBrain;
                else if (this is PredatorAgent && EcosystemManager.Instance.PredatorBrain != null)
                    bp.Model = EcosystemManager.Instance.PredatorBrain;
            }
        }
    }

    public virtual void InitializeStats(AgentStats inherited = null)
    {
        if (stats == null || stats.Generation == 0)
        {
            stats = inherited ?? new AgentStats(
                         Random.Range(10f, AgentStats.maxPossibleSpeed),  // speed
                         Random.Range(1.5f, AgentStats.maxPossibleSize),  // maxSize
                         Random.Range(0.8f, 1.5f), // sightRange (keep in small bounds)
                         Random.Range(90f, AgentStats.maxPossibleLifeTime)); // MaxLifetime
        }

        AgentAnimalBase animal = GetComponent<AgentAnimalBase>();
        if (animal.animalType == AgentAnimalBase.AnimalType.Prey)
            EcosystemManager.Instance.CumulativeData.highestPreyGeneration = Mathf.Max(stats.Generation, EcosystemManager.Instance.CumulativeData.highestPreyGeneration);
        else if (animal.animalType == AgentAnimalBase.AnimalType.Predator)
            EcosystemManager.Instance.CumulativeData.highestPredatorGeneration = Mathf.Max(stats.Generation, EcosystemManager.Instance.CumulativeData.highestPredatorGeneration);

        maxSpeed = stats.speed;
        transform.localScale = Vector3.one * stats.CurrentSize;
        animalBar.SetStats(stats);

        // scale RayPerception length by sightRange
        const float baseRay = 20f; // baseline
        foreach (var sensor in GetComponents<RayPerceptionSensorComponent3D>())
            sensor.RayLength = baseRay * stats.sightRange;
    }

    private Vector3 moveDir = Vector3.zero;
    private Vector3 desiredXZ = Vector3.zero;
    public void FixedUpdate()
    {
        stats.TickDecay(Time.fixedDeltaTime);

        if (!stats.IsAlive)
        {
            AgentAnimalBase animal = GetComponent<AgentAnimalBase>();
            if (stats.LivedFullLife)
            {
                EcosystemManager.Instance.CumulativeData.reachedLifeEnd += 1;
                animal.ReasonOfDeath = AgentAnimalBase.DeathReason.Natural;
            }
            if (stats.hunger <= 0f)
            {
                EcosystemManager.Instance.CumulativeData.diedFromHunger += 1;
                animal.ReasonOfDeath = AgentAnimalBase.DeathReason.Starvation;
            }
            if (stats.thirst <= 0f)
            {
                EcosystemManager.Instance.CumulativeData.diedFromThirst += 1;
                animal.ReasonOfDeath = AgentAnimalBase.DeathReason.Dehydration;
            }
            if (stats.health <= 0f)
            {
                EcosystemManager.Instance.CumulativeData.diedFromHealthOver += 1;
                animal.ReasonOfDeath = AgentAnimalBase.DeathReason.HealthOver;
            }
            Die();
            return;
        }

        RewardUtility.ApplyVitalityReward(this, stats.hunger, stats.thirst);

        // Compute movement
        moveDir.Set(transform.forward.x * currentMove.y, 0f, transform.forward.z * currentMove.y);
        float speedFactor = (1f - brake) * maxSpeed;

        desiredXZ.Set(moveDir.x * speedFactor, rb.velocity.y, moveDir.z * speedFactor);
        rb.velocity = desiredXZ;

        // CustomLogger.Log($"MoveDir: {moveDir}, Brake: {brake:F2}, MaxSpeed: {maxSpeed:F2}, Velocity: {rb.velocity}, PreviousVelocity: {prevVelocity}");
        // Rotation
        if (Mathf.Abs(currentMove.x) > 0.01f)
        {
            float turnSpeed = 120f; // degrees per second
            transform.Rotate(Vector3.up, currentMove.x * turnSpeed * Time.fixedDeltaTime);
        }

        UpdateSize();
    }


    public virtual void UpdateSize() { }
    public virtual void Die()
    {
        // Debug.Log($"Animal is Dead : {gameObject.name}");
    }
}
