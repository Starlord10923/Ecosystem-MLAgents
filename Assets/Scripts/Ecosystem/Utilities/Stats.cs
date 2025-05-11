using Unity.MLAgents;

public static class Stats
{
    static readonly StatsRecorder SR = Academy.Instance.StatsRecorder;

    public static void RecordSurvival(int alive)
        => SR.Add("Environment/Alive", alive, StatAggregationMethod.MostRecent);

    public static void RecordMeanAge(float meanAge)
        => SR.Add("Population/MeanAge", meanAge, StatAggregationMethod.MostRecent);

    public static void RecordEpisodeReturn(float ret)
        => SR.Add("Episode/Return", ret, StatAggregationMethod.MostRecent);
}
