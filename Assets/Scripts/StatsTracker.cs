using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AgentPerformance
{
    public int wins;
    public int losses;
    public float totalScore;
}

public class StatsTracker : Singleton<StatsTracker>
{
    [Header("Global Totals")]
    public int totalEpisodes;
    public int totalWins;
    public int totalLosses;
    public float totalRewardGained;
    public float totalPenaltyLost;

    [Header("Tracked Agents")]
    [SerializeField] private List<AgentController> trackedAgents = new();
    private Dictionary<AgentController, AgentPerformance> AgentPerformanceDict = new();

    [Header("Debug View - [Wins, Losses, TotalScore]")]
    public List<Vector3> Stats = new(); // Each Vector3 = (wins, losses, totalScore)

    public bool RecordStats = true;

    public void RegisterAgent(AgentController agent)
    {
        if (!AgentPerformanceDict.ContainsKey(agent))
        {
            AgentPerformance stats = new AgentPerformance();
            AgentPerformanceDict[agent] = stats;
            trackedAgents.Add(agent);
            Stats.Add(Vector3.zero); // Reserve slot for this agent
        }
    }

    public void RecordWin(AgentController agent)
    {
        if (!RecordStats) return;

        totalEpisodes++;
        totalWins++;

        if (AgentPerformanceDict.TryGetValue(agent, out var stats))
        {
            stats.wins++;

            int index = trackedAgents.IndexOf(agent);
            if (index != -1)
            {
                Stats[index] = new Vector3(stats.wins, stats.losses, stats.totalScore);
            }
        }
    }

    public void RecordLoss(AgentController agent)
    {
        if (!RecordStats) return;

        totalEpisodes++;
        totalLosses++;

        if (AgentPerformanceDict.TryGetValue(agent, out var stats))
        {
            stats.losses++;

            int index = trackedAgents.IndexOf(agent);
            if (index != -1)
            {
                Stats[index] = new Vector3(stats.wins, stats.losses, stats.totalScore);
            }
        }
    }
    public void RecordStep(AgentController agent, float reward)
    {
        if (!RecordStats || reward == 0f) return;

        if (AgentPerformanceDict.TryGetValue(agent, out var stats))
        {
            stats.totalScore += reward;

            int index = trackedAgents.IndexOf(agent);
            if (index != -1)
            {
                Stats[index] = new Vector3(stats.wins, stats.losses, stats.totalScore);
            }
        }

        if (reward > 0)
            totalRewardGained += reward;
        else
            totalPenaltyLost += reward;
    }


    [ContextMenu("Reset Stats")]
    public void ResetStats()
    {
        totalEpisodes = 0;
        totalWins = 0;
        totalLosses = 0;
        totalRewardGained = 0;
        totalPenaltyLost = 0;
    }
}
