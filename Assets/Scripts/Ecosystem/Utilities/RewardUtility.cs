using System.Linq;
using Unity.MLAgents;
using UnityEngine;

public static class RewardUtility
{
    /// <summary>Reward for consuming food. Baseline: +0.2 for 0.1 units.</summary>
    public static void AddNutritionReward(Agent agent, float amount)
    {
        float scaled = 0.2f * amount / 0.1f;
        EcosystemManager.Instance.CumulativeData.totalRewardGiven += scaled;
        agent.AddReward(scaled);
    }

    /// <summary>Reward for drinking water. Baseline: +0.2 for 0.1 units.</summary>
    public static void AddWaterReward(Agent agent, float amount)
    {
        float scaled = 0.2f * amount / 0.1f;
        EcosystemManager.Instance.CumulativeData.totalRewardGiven += scaled;
        agent.AddReward(scaled);
    }

    /// <summary>Reward for predator consuming prey. Baseline: +5.0 for 0.6 units.</summary>
    public static void AddPredationReward(Agent agent, float amount)
    {
        float scaled = 3.0f * amount / 0.6f;
        EcosystemManager.Instance.CumulativeData.totalRewardGiven += scaled;
        agent.AddReward(scaled);
    }

    /// <summary>Reward for Mating. Baseline: +0.2 for delta.</summary>
    public static void AddMatingReward(Agent agent, float amount = 0.15f)
    {
        EcosystemManager.Instance.CumulativeData.totalRewardGiven += amount;
        EcosystemManager.Instance.CumulativeData.partialMatingReward += amount;
        agent.AddReward(amount);
    }

    /// <summary>Group Reward for Mating. Baseline: +5 for involved agents, +0.5 for all others.</summary>
    public static void AddMatingSuccessReward(AgentAnimalBase agentA, AgentAnimalBase agentB, float localReward = 5f, float globalReward = 0.25f)
    {
        var sameClassAgents = EcosystemManager.Instance.liveAgents
            .Select(a => a.GetComponent<AgentAnimalBase>())
            .Where(a => a != null && a != agentA && a != agentB && a.animalType == agentA.animalType);

        EcosystemManager.Instance.CumulativeData.totalRewardGiven += (localReward * 2) + (globalReward * sameClassAgents.Count());
        EcosystemManager.Instance.CumulativeData.totalMating++;

        agentA.AddReward(localReward);
        agentB.AddReward(localReward);

        foreach (var agent in sameClassAgents)
        {
            agent.AddReward(globalReward);
        }
    }

    /// <summary>Negative reward for being hungry or thirsty each frame. Penalty scales with deficiency.</summary>
    public static void ApplyVitalityReward(Agent agent, float hunger, float thirst)
    {
        // --- Reward Case: healthy levels ---
        if (hunger > 0.7f && thirst > 0.7f)
        {
            float reward = (hunger - 0.7f) + (thirst - 0.7f); // max theoretical: (1.0 - 0.7) * 2 = 0.6
            float scaledReward = Mathf.Clamp(reward, 0f, 0.05f) * Time.fixedDeltaTime;

            EcosystemManager.Instance.CumulativeData.totalRewardGiven += scaledReward;
            agent.AddReward(scaledReward);
        }
        // --- Penalty Case: undernourished or dehydrated ---
        else
        {
            float penalty = ComputePenalty(hunger) + ComputePenalty(thirst);  // max ~ -0.2
            float scaledPenalty = Mathf.Clamp(penalty, -0.1f, 0f) * Time.fixedDeltaTime;

            EcosystemManager.Instance.CumulativeData.totalPenaltyGiven += Mathf.Abs(scaledPenalty);
            agent.AddReward(scaledPenalty);
        }
    }

    // Penalty/Rewards based on if agent had children/died naturally
    public static void AddDeathPenalty(AgentAnimalBase agent)
    {
        if (agent.stats.numChildren > 0 && agent.ReasonOfDeath == AgentAnimalBase.DeathReason.Natural)
        {
            EcosystemManager.Instance.CumulativeData.totalPenaltyGiven += agent.stats.numChildren / 2f;
            agent.AddReward(Mathf.Clamp01(agent.stats.numChildren / 2f));
            return;
        }
        if (agent.ReasonOfDeath == AgentAnimalBase.DeathReason.Natural)
        {
            EcosystemManager.Instance.CumulativeData.totalPenaltyGiven += 0.3f;
            agent.AddReward(Mathf.Clamp01(0.3f));
            return;
        }
        EcosystemManager.Instance.CumulativeData.totalPenaltyGiven += 1f;
        agent.AddReward(-1f);
    }

    public static void AddCrowdedPenalty(Agent agent, int count)
    {
        float penalty = Mathf.Clamp01(count / 6f) * 0.05f;  // caps at -0.05
        EcosystemManager.Instance.CumulativeData.totalPenaltyGiven += penalty;
        EcosystemManager.Instance.CumulativeData.crowdingPenalty += penalty;
        agent.AddReward(-penalty);
    }

    /// <summary>Flat death penalty. Called once on death.</summary>
    public static void AddWallHitPenalty(Agent agent)
    {
        EcosystemManager.Instance.CumulativeData.totalPenaltyGiven += 0.5f;
        agent.AddReward(-0.5f);
    }

    private static float ComputePenalty(float value)
    {
        if (value >= 0.7f) return 0f;
        if (value >= 0.3f) return -0.0025f * ((0.7f - value) / 0.1f);
        return -0.01f * ((0.3f - value) / 0.1f);
    }
}
