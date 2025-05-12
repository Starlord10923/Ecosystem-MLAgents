using System.IO;
using UnityEngine;

public class Telemetry : Singleton<Telemetry>
{
    private string csvPath;
    private StreamWriter writer;
    private EpisodeMetrics lastRecordedData;
    private double episodeStartTime = 0.0;

    protected override void Awake()
    {
        base.Awake();

        string telemetryDir = Path.Combine(Application.dataPath, "Telemetry");
        if (!Directory.Exists(telemetryDir))
            Directory.CreateDirectory(telemetryDir);

        csvPath = Path.Combine(telemetryDir, $"eco-log-{System.DateTime.Now:yyyyMMdd_HHmm}.csv");
        writer = new StreamWriter(csvPath);

        writer.WriteLine("episode,startTime,endTime,aliveTime," +
                         "totalRewardGiven,totalPenaltyGiven,crowdingPenalty," +
                         "totalPreySpawned,totalPredatorsSpawned," +
                         "foodConsumed,waterConsumed," +
                         "totalMating,partialMatingReward," +
                         "animalKilled,reachedLifeEnd,diedFromHunger,diedFromThirst");

        Debug.Log($"[Telemetry] Logging to: {csvPath}");
    }

    public void OnEpisodeStart()
    {
        episodeStartTime = Time.timeAsDouble;
    }

    public void OnEpisodeEnd(EpisodeMetrics snapshot)
    {
        double end = Time.timeAsDouble;

        // First episode → log raw snapshot
        if (lastRecordedData == null)
        {
            lastRecordedData = snapshot.Clone();
            writer.WriteLine($"{snapshot.totalEpisodes},{episodeStartTime},{end:F2},{end - episodeStartTime:F2}," +
                             $"{snapshot.totalRewardGiven:F3}," +
                             $"{snapshot.totalPenaltyGiven:F3}," +
                             $"{snapshot.crowdingPenalty:F3}," +
                             $"{snapshot.totalPreySpawned}," +
                             $"{snapshot.totalPredatorsSpawned}," +
                             $"{snapshot.foodConsumed}," +
                             $"{snapshot.waterConsumed}," +
                             $"{snapshot.totalMating}," +
                             $"{snapshot.partialMatingReward:F3}," +
                             $"{snapshot.animalKilled}," +
                             $"{snapshot.reachedLifeEnd}," +
                             $"{snapshot.diedFromHunger}," +
                             $"{snapshot.diedFromThirst}");
        }
        else
        {
            double aliveTime = end - episodeStartTime;
            writer.WriteLine($"{snapshot.totalEpisodes},{episodeStartTime:F2},{end:F2},{aliveTime:F2}," +
                             $"{(snapshot.totalRewardGiven - lastRecordedData.totalRewardGiven):F3}," +
                             $"{(snapshot.totalPenaltyGiven - lastRecordedData.totalPenaltyGiven):F3}," +
                             $"{(snapshot.crowdingPenalty - lastRecordedData.crowdingPenalty):F3}," +
                             $"{snapshot.totalPreySpawned - lastRecordedData.totalPreySpawned}," +
                             $"{snapshot.totalPredatorsSpawned - lastRecordedData.totalPredatorsSpawned}," +
                             $"{snapshot.foodConsumed - lastRecordedData.foodConsumed}," +
                             $"{snapshot.waterConsumed - lastRecordedData.waterConsumed}," +
                             $"{snapshot.totalMating - lastRecordedData.totalMating}," +
                             $"{(snapshot.partialMatingReward - lastRecordedData.partialMatingReward):F3}," +
                             $"{snapshot.animalKilled - lastRecordedData.animalKilled}," +
                             $"{snapshot.reachedLifeEnd - lastRecordedData.reachedLifeEnd}," +
                             $"{snapshot.diedFromHunger - lastRecordedData.diedFromHunger}," +
                             $"{snapshot.diedFromThirst - lastRecordedData.diedFromThirst}");
        }

        lastRecordedData = snapshot.Clone(); // move reference forward
        writer.Flush();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        writer?.Close();
    }

    public EpisodeMetrics GetLastSnapshot()
    {
        return lastRecordedData;
    }
}
