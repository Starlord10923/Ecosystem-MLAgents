using UnityEngine;

public static class GeneticUtility
{
    private static float InheritGene(float a, float b, float fitnessA, float fitnessB, float mutationRange = 0.05f)
    {
        float totalFitness = fitnessA + fitnessB + 1e-6f; // prevent div0
        float weight = fitnessA / totalFitness; // favor better parent

        float baseValue = Mathf.Lerp(a, b, weight);
        float mutation = Random.Range(-mutationRange, mutationRange);

        // Rare large mutation event
        if (Random.value < 0.01f)
            mutation *= Random.Range(3f, 5f);

        return baseValue + mutation;
    }

    public static AgentStats Inherit(AgentStats p1, AgentStats p2, float rewardA, float rewardB)
    {
        float speed = Mathf.Clamp(InheritGene(p1.speed, p2.speed, rewardA, rewardB), 5f, 25f);
        float maxSize = Mathf.Clamp(InheritGene(p1.maxSize, p2.maxSize, rewardA, rewardB), 1f, 3.5f);
        float sightRange = Mathf.Clamp(InheritGene(p1.sightRange, p2.sightRange, rewardA, rewardB), 0.5f, 3f);
        float maxLifetime = Mathf.Clamp(InheritGene(p1.MaxLifetime, p2.MaxLifetime, rewardA, rewardB), 30f, 100f);

        var child = new AgentStats(speed, maxSize, sightRange, maxLifetime);
        child.Generation = Mathf.Max(p1.Generation, p2.Generation) + 1;
        return child;
    }
}
