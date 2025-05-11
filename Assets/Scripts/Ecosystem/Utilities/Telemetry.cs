using System.IO;
using UnityEngine;

public class Telemetry : Singleton<Telemetry>
{
    string csvPath;
    StreamWriter writer;

    protected override void Awake()
    {
        base.Awake();

        string telemetryDir = Path.Combine(Application.dataPath, "Telemetry");
        if (!Directory.Exists(telemetryDir))
            Directory.CreateDirectory(telemetryDir);

        csvPath = Path.Combine(telemetryDir, $"ecosim_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv");

        writer = new StreamWriter(csvPath);
        writer.WriteLine("episode,startTime,endTime,aliveTime," +
                         "totalRewardGiven,totalPenaltyGiven,crowdingPenalty," +
                         "totalPreySpawned,totalPredatorsSpawned," +
                         "foodConsumed,waterConsumed," +
                         "totalMating,partialMatingReward," +
                         "animalKilled,reachedLifeEnd,diedFromHunger,diedFromThirst");

        Debug.Log($"[Telemetry] Logging to: {csvPath}");
    }

    public void OnEpisodeEnd(EcosystemManager eco)
    {
        var data = eco.CumulativeData;
        double end = Time.timeAsDouble;

        writer.WriteLine($"{data.totalEpisodes},{(eco.enabled ? 0.0 : 0.0)},{end:F2},{end:F2}," +
                         $"{data.totalRewardGiven:F3},{data.totalPenaltyGiven:F3},{data.crowdingPenalty:F3}," +
                         $"{data.totalPreySpawned},{data.totalPredatorsSpawned}," +
                         $"{data.foodConsumed},{data.waterConsumed}," +
                         $"{data.totalMating},{data.partialMatingReward:F3}," +
                         $"{data.animalKilled},{data.reachedLifeEnd},{data.diedFromHunger},{data.diedFromThirst}");

        writer.Flush();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        writer?.Close();
    }
}
