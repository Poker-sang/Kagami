using Kagami.Core;
using System.Text.Json;

namespace Kagami.Services;

public static class Luck
{
    private static string[] routine = GetRoutine();

    public static void Refresh() => routine = GetRoutine();

    private static string[] GetRoutine()
    {
        try
        {
            return JsonSerializer.Deserialize<string[]>(File.OpenRead(Paths.UtilitiesPath + "luck.json")) ??
                   Array.Empty<string>();
        }
        catch (Exception)
        {
            return Array.Empty<string>();
        }
    }

    public struct Value
    {
        public string Draw { get; init; }
        public string[] Should { get; init; }
        public string[] Avoid { get; init; }
    }

    public static Value GetValue(long uin)
    {
        var seed = (int)((DateTime.Today.Ticks / TimeSpan.TicksPerDay) + (uin % int.MaxValue));
        var random = new Random(seed);
        var draw = random.Next(100) switch
        {
            < 15 => "凶",
            < 30 => "末吉",
            < 60 => "小吉",
            < 90 => "中吉",
            _ => "大吉",
        };
        var daily = new SortedSet<string>();
        while (daily.Count < 6)
            _ = daily.Add(routine[random.Next(routine.Length)]);
        return new Value
        {
            Draw = draw,
            Should = daily.ToArray()[..3],
            Avoid = daily.ToArray()[^3..]
        };
    }
}
