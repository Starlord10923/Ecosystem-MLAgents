using Unity.MLAgents;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AgentBase : Agent
{
    protected Rigidbody rb;
    protected Vector2 currentMove;
    protected float brake;
    public AgentStats stats;

    public float maxSpeed = 3f;

    protected void Awake() => rb = GetComponent<Rigidbody>();

    public void InitializeStats(AgentStats inherited = null)
    {
        stats = inherited ?? new AgentStats(Random.Range(2f, 4f), Random.Range(0.8f, 1.2f), Random.Range(8f, 12f));
        transform.localScale = Vector3.one * stats.size;
        maxSpeed = stats.speed;
    }

    protected void FixedUpdate()
    {
        if (!stats.IsAlive)
        {
            Die();
            return;
        }

        stats.TickDecay(Time.fixedDeltaTime);
        RewardManager.Instance.EvaluatePenalty(this);

        if (Physics.Raycast(transform.position, Vector3.down, 1.1f, LayerMask.GetMask("Ground")))
        {
            Vector3 moveDir = transform.TransformDirection(new Vector3(currentMove.x, 0, currentMove.y));
            Vector3 desiredVel = moveDir.normalized * maxSpeed * (1f - brake);
            rb.velocity = new Vector3(desiredVel.x, rb.velocity.y, desiredVel.z);

            if (moveDir != Vector3.zero)
            {
                Quaternion rot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, 10f * Time.fixedDeltaTime);
            }
        }
    }

    protected virtual void Die(){}
}
