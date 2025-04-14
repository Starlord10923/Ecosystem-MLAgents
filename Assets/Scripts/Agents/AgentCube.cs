using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class AgentCube : AgentController
{
    [SerializeField] protected Transform target;

    public override void OnEpisodeBegin()
    {
        // Reset agent & target positions
        transform.localPosition = new Vector3(Random.Range(-5f, 5f), 0.3f, Random.Range(-5f, 5f));
        target.localPosition = new Vector3(Random.Range(-5f, 5f), 0.3f, Random.Range(-5f, 5f));
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent position + Target position
        sensor.AddObservation(target.localPosition - transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float horizontal = actions.ContinuousActions[0];
        float vertical = actions.ContinuousActions[1];

        Vector3 move = force * Time.deltaTime * new Vector3(horizontal, 0, vertical);
        transform.localPosition += move;

        // Small time penalty to encourage faster completion
        GetPoints(-0.001f);
        float distanceToTarget = Vector3.Distance(transform.localPosition, target.localPosition);
        GetPoints(-distanceToTarget * 0.001f); // Closer is better

        if (Mathf.Abs(transform.localPosition.x) >= 4.5 || Mathf.Abs(transform.localPosition.z) >= 4.5)
            Lose();
    }
}
