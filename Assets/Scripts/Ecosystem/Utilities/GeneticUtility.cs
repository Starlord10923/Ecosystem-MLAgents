using UnityEngine;

public static class GeneticUtility
{
    public static float Blend(float a, float b, float mutation = 0.05f)
    {
        float baseValue = Mathf.Lerp(a, b, Random.value);
        return baseValue + Random.Range(-mutation, mutation);
    }

    public static AgentStats Inherit(AgentStats p1, AgentStats p2)
    {
        return new AgentStats(
            Blend(p1.speed, p2.speed),
            Blend(p1.size, p2.size),
            Blend(p1.sightRange, p2.sightRange)
        );
    }
}
