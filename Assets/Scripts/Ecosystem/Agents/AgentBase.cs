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

    protected virtual void Awake() => rb = GetComponent<Rigidbody>();

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
            AddReward(-1f);
            Die();
            return;
        }

        RewardManager.Instance.EvaluatePenalty(this);

        Vector3 moveDir = transform.TransformDirection(new Vector3(currentMove.x, 0, currentMove.y));
        Vector3 desiredVel = (1f - brake) * maxSpeed * moveDir.normalized;

        if (Physics.Raycast(transform.position, Vector3.down, 1.1f, LayerMask.GetMask("Ground")))
        {
            rb.velocity = new Vector3(desiredVel.x, rb.velocity.y, desiredVel.z);

            if (moveDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10f * Time.fixedDeltaTime);
            }
        }

        transform.localScale = Vector3.one * stats.CurrentSize;
    }

    public virtual void Die() { }
}
