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
    protected override void OnDisable()
    {
        base.OnDisable();
        EcosystemManager.Instance.Unregister(this);
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animalBar = GetComponentInChildren<AnimalBar>();
    }

    public virtual void InitializeStats(AgentStats inherited = null)
    {
        stats = inherited ?? new AgentStats(
                     Random.Range(2f, 4f),  // speed
                     Random.Range(1.5f, 2.5f),  // maxSize
                     Random.Range(0.8f, 1.2f)); // sightRange (keep in small bounds)

        maxSpeed = stats.speed;
        transform.localScale = Vector3.one * stats.CurrentSize;
        animalBar.SetStats(stats);

        // scale RayPerception length by sightRange
        const float baseRay = 20f; // baseline
        foreach (var sensor in GetComponents<RayPerceptionSensorComponent3D>())
            sensor.RayLength = baseRay * stats.sightRange;
    }

    public virtual void FixedUpdate()
    {
        stats.TickDecay(Time.fixedDeltaTime);

        if (!stats.IsAlive)
        {
            Die();
            return;
        }

        RewardUtility.AddDecayPenalty(this, stats.hunger, stats.thirst);

        Vector3 moveDir = transform.TransformDirection(new Vector3(currentMove.x, 0, currentMove.y));
        Vector3 desiredXZ = (1f - brake) * maxSpeed * moveDir.normalized;

        if (Physics.Raycast(transform.position, Vector3.down, 1.1f, LayerMask.GetMask("Ground")))
        {
            rb.velocity = new Vector3(desiredXZ.x, rb.velocity.y, desiredXZ.z); // ← keep vertical component

            if (moveDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.fixedDeltaTime);
            }
        }

        UpdateSize();
    }

    public virtual void UpdateSize() { }
    public virtual void Die()
    {
        Telemetry.Instance.OnAgentDeath();
        Telemetry.Instance.AddReward(GetCumulativeReward());
    }
}
