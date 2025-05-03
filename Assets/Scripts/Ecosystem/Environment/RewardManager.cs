using Unity.VisualScripting;
using UnityEngine;

public class RewardManager : Singleton<RewardManager>
{
    public void EvaluatePenalty(AgentBase agent)
    {
        float hunger = agent.stats.hunger;
        float thirst = agent.stats.thirst;

        float penalty = 0f;
        penalty += GetPenalty(hunger);
        penalty += GetPenalty(thirst);

        agent.AddReward(penalty * Time.fixedDeltaTime);
    }

    private float GetPenalty(float value)
    {
        if (value >= 0.7f) return 0f;
        else if (value >= 0.3f) return -0.0025f * ((0.7f - value) / 0.1f);
        else return -0.01f * ((0.3f - value) / 0.1f);
    }
}
