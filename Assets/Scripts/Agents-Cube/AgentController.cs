using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AgentController : Agent
{
    public float force = 10f;

    [SerializeField] protected Color WinColor = Color.green;
    [SerializeField] protected Color LoseColor = Color.red;
    
    [SerializeField] protected MeshRenderer floorMeshRenderer;

    private void Start()
    {
        // Register this agent instance with StatsTracker
        StatsTracker.Instance.RegisterAgent(this);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuous = actionsOut.ContinuousActions;
        continuous[0] = Input.GetAxis("Horizontal");
        continuous[1] = Input.GetAxis("Vertical");

        // Debug.Log($"Heuristic Input: {continuous[0]} | {continuous[1]}");
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            Lose();
        }

        if (other.gameObject.CompareTag("Target"))
        {
            Win();
        }
    }

    protected void Win()
    {
        float reward = 10.0f;
        GetPoints(reward);
        StatsTracker.Instance.RecordWin(this);
        floorMeshRenderer.material.color = WinColor;
        EndEpisode();
    }

    protected void Lose()
    {
        float penalty = -0.5f;
        GetPoints(penalty);
        StatsTracker.Instance.RecordLoss(this);
        floorMeshRenderer.material.color = LoseColor;
        EndEpisode();
    }

    public void GetPoints(float points = 1f)
    {
        AddReward(points);
        StatsTracker.Instance.RecordStep(this, points);
    }
}
