using Unity.MLAgents;

public class RewardUtility : Singleton<RewardUtility>
{
    /// <summary>Reward for consuming food. Baseline: +0.2 for 0.1 units.</summary>
    public void AddNutritionReward(Agent agent, float amount)
    {
        float scaled = 0.2f * amount / 0.1f;
        agent.AddReward(scaled);
    }

    /// <summary>Reward for drinking water. Baseline: +0.2 for 0.1 units.</summary>
    public void AddWaterReward(Agent agent, float amount)
    {
        float scaled = 0.2f * amount / 0.1f;
        agent.AddReward(scaled);
    }

    /// <summary>Reward for predator consuming prey. Baseline: +1.0 for 0.6 units.</summary>
    public void AddPredationReward(Agent agent, float amount)
    {
        float scaled = 1.0f * amount / 0.6f;
        agent.AddReward(scaled);
    }

    /// <summary>Negative reward for being hungry or thirsty each frame. Penalty scales with deficiency.</summary>
    public void AddDecayPenalty(Agent agent, float hunger, float thirst)
    {
        float penalty = 0f;

        penalty += ComputePenalty(hunger);
        penalty += ComputePenalty(thirst);

        agent.AddReward(penalty * UnityEngine.Time.fixedDeltaTime);
    }

    /// <summary>Flat death penalty. Called once on death.</summary>
    public void AddDeathPenalty(Agent agent)
    {
        agent.AddReward(-1f);
    }

    /// <summary>Custom reward value for debugging or sparse events.</summary>
    public void AddCustom(Agent agent, float value)
    {
        agent.AddReward(value);
    }

    private float ComputePenalty(float value)
    {
        if (value >= 0.7f) return 0f;
        if (value >= 0.3f) return -0.0025f * ((0.7f - value) / 0.1f);
        return -0.01f * ((0.3f - value) / 0.1f);
    }
}
