using System.Reflection;
using Konata.Core;

namespace Kagami.Services;

public static class KonataBuildStamp
{
    public static string Branch => s_stamp[0];

    public static string CommitHash => s_stamp[1][..16];

    public static string BuildTime => s_stamp[2];

    public static string Version { get; } = typeof(Bot).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

    private static readonly string[] s_stamp
        = typeof(Bot).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .First(x => x.Key is "BuildStamp").Value!.Split(";");
}
public static class KagamiBuildStamp
{
    private static readonly Dictionary<string, string> s_list = typeof(KagamiBuildStamp)
        .Assembly
        .GetCustomAttributes<AssemblyMetadataAttribute>()
        .ToDictionary(
            i => i.Key,
            i => i.Value!);
    public static string Branch { get; } = s_list[nameof(Branch)];

    public static string Revision { get; } = s_list[nameof(Revision)];

    public static string? Version { get; } = typeof(KagamiBuildStamp).Assembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version;

    public static DateTime BuildTime { get; } = DateTime.Parse(s_list[nameof(BuildTime)]);

}
