using System.Collections.Generic;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AgentCubeMaze : AgentController
{
    [SerializeField] List<Transform> targets;

    Vector3 agentStartPosition;
    List<Vector3> targetStartPositions;

    public int collected = 0;

    private void Awake()
    {
        agentStartPosition = transform.localPosition;
        targetStartPositions = new();
        foreach (var target in targets)
            targetStartPositions.Add(target.localPosition);
    }
    public override void OnEpisodeBegin()
    {
        // Reset agent & target positions
        transform.localPosition = agentStartPosition;
        collected = 0;
        NextCheckPoint();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent position + Target position
        sensor.AddObservation(targets[collected % targets.Count].localPosition - transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float horizontal = actions.ContinuousActions[0];
        float vertical = actions.ContinuousActions[1];

        Vector3 move = force * Time.deltaTime * new Vector3(horizontal, 0, vertical);
        transform.localPosition += move;

        // Small time penalty to encourage faster completion
        GetPoints(-0.001f);
        float distanceToTarget = Vector3.Distance(transform.localPosition, targets[collected % targets.Count].localPosition);
        GetPoints(-distanceToTarget * 0.001f); // Closer is better

        if (Mathf.Abs(transform.localPosition.x) >= 4.5 || Mathf.Abs(transform.localPosition.z) >= 4.5)
            Lose();
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            Lose();
        }

        if (other.gameObject.CompareTag("Target") && targets[collected % targets.Count] == other.transform)
        {
            ClearCheckpoint();
        }
    }

    private void ClearCheckpoint()
    {
        GetPoints(5f);
        targets[collected % targets.Count].gameObject.SetActive(false);
        collected += 1;
        NextCheckPoint();

        float progress = (float)collected / targets.Count;
        floorMeshRenderer.material.color = Color.Lerp(Color.black, WinColor, progress);

        Debug.Log("Collected : " + collected);

        if (collected == targets.Count)
        {
            Win();
        }
    }

    private void NextCheckPoint()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].gameObject.SetActive(i == collected);
            targets[i].localPosition = targetStartPositions[i];
        }
    }
}
