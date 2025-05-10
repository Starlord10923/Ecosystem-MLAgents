using UnityEngine;

public static class GeneticUtility
{
    public static float Blend(float a, float b, float mutationRange = 0.05f)
    {
        float baseValue = Mathf.Lerp(a, b, Random.value);
        return baseValue + Random.Range(-mutationRange, mutationRange);
    }

    public static AgentStats Inherit(AgentStats p1, AgentStats p2)
    {
        return new AgentStats(
            Blend(p1.speed, p2.speed),
            Blend(p1.maxSize, p2.maxSize),
            Blend(p1.sightRange, p2.sightRange)
        );
    }
}
