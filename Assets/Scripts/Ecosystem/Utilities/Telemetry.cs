// Utilities/Telemetry.cs
using System.IO;
using Unity.MLAgents;
using UnityEngine;

public class Telemetry : Singleton<Telemetry>
{
    string csvPath;
    StreamWriter writer;

    protected override void Awake()
    {
        base.Awake();

        // Create folder if it doesn't exist
        string telemetryDir = Path.Combine(Application.dataPath, "Telemetry");
        if (!Directory.Exists(telemetryDir))
            Directory.CreateDirectory(telemetryDir);

        csvPath = Path.Combine(telemetryDir, $"ecosim_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv");

        writer = new StreamWriter(csvPath);
        writer.WriteLine("episode,startTime,endTime,aliveTime,spawned,dead,rewardSum");

        Debug.Log($"[Telemetry] Logging to: {csvPath}");
    }

    /* ---------- environment level ---------- */
    public int EpisodeIndex { get; private set; } = 0;
    double episodeStart = 0.0;
    int spawned = 0;
    int dead = 0;
    float rewardSum = 0f;

    public void OnEpisodeBegin()
    {
        EpisodeIndex++;
        episodeStart = Time.timeAsDouble;
        spawned = dead = 0;
        rewardSum = 0;
    }

    public void OnAgentSpawn() => spawned++;
    public void OnAgentDeath() => dead++;

    public void AddReward(float r) => rewardSum += r;

    public void OnEpisodeEnd()
    {
        double end = Time.timeAsDouble;
        writer.WriteLine($"{EpisodeIndex},{episodeStart:F2},{end:F2},{end - episodeStart:F2}," +
                         $"{spawned},{dead},{rewardSum:F3}");
        writer.Flush();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        writer?.Close();
    }
}
