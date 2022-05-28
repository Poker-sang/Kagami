using Konata.Core;
using System.Reflection;

namespace Kagami.Services;

public static class KonataBuildStamp
{
    public static string Branch => sStamp[0];

    public static string CommitHash => sStamp[1][..16];

    public static string BuildTime => sStamp[2];

    public static string Version { get; } = typeof(Bot).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;

    private static readonly string[] sStamp
        = typeof(Bot).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .First(x => x.Key is "BuildStamp").Value!.Split(";");
}
public static class KagamiBuildStamp
{
    private static readonly Dictionary<string, string> sList = typeof(KagamiBuildStamp)
        .Assembly
        .GetCustomAttributes<AssemblyMetadataAttribute>()
        .ToDictionary(
            i => i.Key,
            i => i.Value!);
    public static string Branch { get; } = sList[nameof(Branch)];

    public static string Revision { get; } = sList[nameof(Revision)];

    public static string? Version { get; } = typeof(KagamiBuildStamp).Assembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version;

    public static DateTime BuildTime { get; } = DateTime.Parse(sList[nameof(BuildTime)]);

}
