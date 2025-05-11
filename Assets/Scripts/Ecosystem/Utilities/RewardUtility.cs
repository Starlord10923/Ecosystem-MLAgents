using Unity.MLAgents;
using UnityEngine;

public static class RewardUtility
{
    /// <summary>Reward for consuming food. Baseline: +0.2 for 0.1 units.</summary>
    public static void AddNutritionReward(Agent agent, float amount)
    {
        float scaled = 0.2f * amount / 0.1f;
        EcosystemManager.Instance.totalRewardGiven += scaled;
        agent.AddReward(scaled);
    }

    /// <summary>Reward for drinking water. Baseline: +0.2 for 0.1 units.</summary>
    public static void AddWaterReward(Agent agent, float amount)
    {
        float scaled = 0.2f * amount / 0.1f;
        EcosystemManager.Instance.totalRewardGiven += scaled;
        agent.AddReward(scaled);
    }

    /// <summary>Reward for predator consuming prey. Baseline: +5.0 for 0.6 units.</summary>
    public static void AddPredationReward(Agent agent, float amount)
    {
        float scaled = 5.0f * amount / 0.6f;
        EcosystemManager.Instance.totalRewardGiven += scaled;
        agent.AddReward(scaled);
    }

    /// <summary>Reward for Mating. Baseline: +0.2 for delta.</summary>
    public static void AddMatingReward(Agent agent, float amount = 0.15f)
    {
        EcosystemManager.Instance.totalRewardGiven += amount;
        EcosystemManager.Instance.partialMatingReward += amount;
        agent.AddReward(amount);
    }
    /// <summary>Reward for Mating. Baseline: +5 for success.</summary>
    public static void AddMatingSuccessReward(Agent agent, float amount = 5f)
    {
        EcosystemManager.Instance.totalRewardGiven += amount;
        EcosystemManager.Instance.totalMating++;
        agent.AddReward(amount);
    }

    /// <summary>Negative reward for being hungry or thirsty each frame. Penalty scales with deficiency.</summary>
    public static void AddDecayPenalty(Agent agent, float hunger, float thirst)
    {
        float penalty = ComputePenalty(hunger) + ComputePenalty(thirst);
        penalty = Mathf.Clamp(penalty, -0.1f, 0f);
        float scaledPenalty = penalty * Time.fixedDeltaTime;
        EcosystemManager.Instance.totalPenaltyGiven += Mathf.Abs(scaledPenalty);
        agent.AddReward(scaledPenalty);
    }

    public static void AddDeathPenalty(Agent agent)
    {
        EcosystemManager.Instance.totalPenaltyGiven += 1f;
        agent.AddReward(-1f);
    }

    /// <summary>Flat death penalty. Called once on death.</summary>
    public static void AddWallHitPenalty(Agent agent)
    {
        EcosystemManager.Instance.totalPenaltyGiven += 0.5f;
        agent.AddReward(-0.5f);
    }

    private static float ComputePenalty(float value)
    {
        if (value >= 0.7f) return 0f;
        if (value >= 0.3f) return -0.0025f * ((0.7f - value) / 0.1f);
        return -0.01f * ((0.3f - value) / 0.1f);
    }
}
