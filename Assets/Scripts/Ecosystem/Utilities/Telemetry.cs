using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Telemetry : Singleton<Telemetry>
{
    private string csvPath;
    private StreamWriter writer;
    private EpisodeMetrics lastRecordedData;
    private double episodeStartTime = 0.0;

    private string agentCsvPath;
    private StreamWriter agentWriter;
    private List<string> agentStatsBuffer = new();

    private void Start()
    {
        int suffix = -1;
#if UNITY_EDITOR
        string rootPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../"));
#else
        string rootPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../../"));
        suffix = System.Diagnostics.Process.GetCurrentProcess().Id;
#endif

        string telemetryDir = Path.Combine(rootPath, "Assets", "Telemetry");
        if (!Directory.Exists(telemetryDir))
            Directory.CreateDirectory(telemetryDir);
        string environmentDir = Path.Combine(telemetryDir, EcosystemManager.Instance.Environment.name);
        if (!Directory.Exists(environmentDir))
            Directory.CreateDirectory(environmentDir);
        string runDir = suffix == -1
                ? Path.Combine(environmentDir, $"{System.DateTime.Now:yyyyMMdd_HHmm}")
                : Path.Combine(environmentDir, $"{System.DateTime.Now:yyyyMMdd_HHmm}_{suffix}");
        if (!Directory.Exists(runDir))
            Directory.CreateDirectory(runDir);

        csvPath = Path.Combine(runDir, $"eco-log.csv");
        writer = new StreamWriter(csvPath);
        writer.WriteLine("episode,startTime,endTime,aliveTime," +
                         "totalRewardGiven,totalPenaltyGiven,crowdingPenalty," +
                         "totalPreySpawned,totalPredatorsSpawned," +
                         "maxPreyGeneration,maxPredatorGeneration," +
                         "foodConsumed,waterConsumed," +
                         "totalMating,partialMatingReward," +
                         "animalKilled,reachedLifeEnd,diedFromHunger,diedFromThirst,diedFromHealthOver");
        writer.Flush(); // ✅ Force write to disk

        agentCsvPath = Path.Combine(runDir, $"agentStats.csv");
        agentWriter = new StreamWriter(agentCsvPath);
        agentWriter.WriteLine("episode,agentID,type,parents,generation,speed,sightRange,maxSize,maxLifetime,age,reasonOfDeath,lifetimeScore");

        agentWriter.Flush();

        Debug.Log($"[Telemetry] Logging to: {csvPath}");
    }

    public void OnEpisodeStart()
    {
        episodeStartTime = Time.timeAsDouble;
    }

    public void OnEpisodeEnd(EpisodeMetrics snapshot)
    {
        double end = Time.timeAsDouble;
        double aliveTime = end - episodeStartTime;

        // First episode → log raw snapshot
        if (lastRecordedData == null)
        {
            lastRecordedData = snapshot.Clone();
            writer.WriteLine($"{snapshot.totalEpisodes},{episodeStartTime:F2},{end:F2},{aliveTime:F2}," +
                             $"{snapshot.totalRewardGiven:F3}," +
                             $"{snapshot.totalPenaltyGiven:F3}," +
                             $"{snapshot.crowdingPenalty:F3}," +
                             $"{snapshot.totalPreySpawned}," +
                             $"{snapshot.totalPredatorsSpawned}," +
                             $"{snapshot.highestPreyGeneration}," +
                             $"{snapshot.highestPredatorGeneration}," +
                             $"{snapshot.foodConsumed}," +
                             $"{snapshot.waterConsumed}," +
                             $"{snapshot.totalMating}," +
                             $"{snapshot.partialMatingReward:F3}," +
                             $"{snapshot.animalKilled}," +
                             $"{snapshot.reachedLifeEnd}," +
                             $"{snapshot.diedFromHunger}," +
                             $"{snapshot.diedFromThirst},"+
                             $"{snapshot.diedFromHealthOver}");
        }
        else
        {
            writer.WriteLine($"{snapshot.totalEpisodes},{episodeStartTime:F2},{end:F2},{aliveTime:F2}," +
                             $"{(snapshot.totalRewardGiven - lastRecordedData.totalRewardGiven):F3}," +
                             $"{(snapshot.totalPenaltyGiven - lastRecordedData.totalPenaltyGiven):F3}," +
                             $"{(snapshot.crowdingPenalty - lastRecordedData.crowdingPenalty):F3}," +
                             $"{snapshot.totalPreySpawned - lastRecordedData.totalPreySpawned}," +
                             $"{snapshot.totalPredatorsSpawned - lastRecordedData.totalPredatorsSpawned}," +
                             $"{snapshot.highestPreyGeneration}," +
                             $"{snapshot.highestPredatorGeneration}," +
                             $"{snapshot.foodConsumed - lastRecordedData.foodConsumed}," +
                             $"{snapshot.waterConsumed - lastRecordedData.waterConsumed}," +
                             $"{snapshot.totalMating - lastRecordedData.totalMating}," +
                             $"{(snapshot.partialMatingReward - lastRecordedData.partialMatingReward):F3}," +
                             $"{snapshot.animalKilled - lastRecordedData.animalKilled}," +
                             $"{snapshot.reachedLifeEnd - lastRecordedData.reachedLifeEnd}," +
                             $"{snapshot.diedFromHunger - lastRecordedData.diedFromHunger}," +
                             $"{snapshot.diedFromThirst - lastRecordedData.diedFromThirst},"+
                             $"{snapshot.diedFromHealthOver - lastRecordedData.diedFromHealthOver}");
        }

        lastRecordedData = snapshot.Clone(); // move reference forward
        writer.Flush();

        FlushAgentStats();
    }

    public void LogAgentStats(AgentAnimalBase agent)
    {
        string type = agent.animalType == AgentAnimalBase.AnimalType.Prey ? "Prey" :
                      agent.animalType == AgentAnimalBase.AnimalType.Predator ? "Predator" : "Unknown";
        EpisodeMetrics EM = EcosystemManager.Instance.CumulativeData;
        var stats = agent.stats;
        string parentIDs = (agent.Parent1ID == agent.Parent2ID)
            ? "-1"
            : $"[{agent.Parent1ID} {agent.Parent2ID}]";

        agentStatsBuffer.Add($"{EM.totalEpisodes},{agent.GetInstanceID()},{type}," +
                             $"{parentIDs}," +
                             $"{stats.Generation}," +
                             $"{stats.speed:F3},{stats.sightRange:F3},{stats.maxSize:F3}," +
                             $"{stats.MaxLifetime:F3},{stats.age:F3}," +
                             $"{agent.ReasonOfDeath},{agent.GetCumulativeReward()}");
    }

    public void FlushAgentStats()
    {
        if (agentStatsBuffer.Count == 0)
            return;
        foreach (var line in agentStatsBuffer)
            agentWriter.WriteLine(line);

        agentWriter.Flush();
        agentStatsBuffer.Clear();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        writer?.Close();
        agentWriter?.Close();
    }

    public EpisodeMetrics GetLastSnapshot()
    {
        return lastRecordedData;
    }
}
