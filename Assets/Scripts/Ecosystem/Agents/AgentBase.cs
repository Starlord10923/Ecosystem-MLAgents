using Unity.MLAgents;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AgentBase : Agent
{
    protected Rigidbody rb;
    protected Vector2 currentMove;
    protected float brake;
    public AgentStats stats;
    public float maxSpeed;
    [SerializeField] AnimalBar animalBar;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animalBar = GetComponentInChildren<AnimalBar>();
    }

    public virtual void InitializeStats(AgentStats inherited = null)
    {
        stats = inherited ?? new AgentStats(Random.Range(2f, 4f), Random.Range(1.5f, 2.5f), Random.Range(8f, 12f));
        maxSpeed = stats.speed;
        transform.localScale = Vector3.one * stats.CurrentSize;
    }

    public virtual void FixedUpdate()
    {
        stats.TickDecay(Time.fixedDeltaTime);

        if (!stats.IsAlive)
        {
            Die();
            return;
        }

        RewardUtility.Instance.AddDecayPenalty(this, stats.hunger, stats.thirst);

        Vector3 moveDir = transform.TransformDirection(new Vector3(currentMove.x, 0, currentMove.y));
        Vector3 desiredVel = (1f - brake) * maxSpeed * moveDir.normalized;

        if (Physics.Raycast(transform.position, Vector3.down, 1.1f, LayerMask.GetMask("Ground")))
        {
            rb.velocity = new Vector3(desiredVel.x, 0f, desiredVel.z);

            if (moveDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.fixedDeltaTime);
            }
        }

        UpdateSize();
    }

    public virtual void UpdateSize() { }
    public virtual void Die() { }
}
