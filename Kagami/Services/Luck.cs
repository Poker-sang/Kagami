﻿using System.Text.Json;

namespace Kagami.Services;

public static class Luck
{
    private const string Uri = "https://poker.blob.core.windows.net/luck/luck.json";

    private static string[] _routine = Array.Empty<string>();

    public static async void Refresh() => _routine = await GetRoutine();

    private static async Task<string[]> GetRoutine()
    {
        try
        {
            return JsonSerializer.Deserialize<string[]>(await Uri.DownloadStringAsync()) ??
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
        if (_routine.Length < 6)
            return new Value
            {
                Draw = "luck.json未获取或获取失败",
                Should = Array.Empty<string>(),
                Avoid = Array.Empty<string>()
            };
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
            _ = daily.Add(_routine[random.Next(_routine.Length)]);
        return new Value
        {
            Draw = draw,
            Should = daily.ToArray()[..3],
            Avoid = daily.ToArray()[^3..]
        };
    }
}
