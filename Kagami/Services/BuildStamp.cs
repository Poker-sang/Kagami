using Konata.Core;
using System.Reflection;

namespace Kagami.Services;

public static class KonataBuildStamp
{
    public static string Branch => Stamp[0];

    public static string CommitHash => Stamp[1][..16];

    public static string BuildTime => Stamp[2];

    public static string Version => InformationalVersion;

    private static readonly string[] Stamp
        = typeof(Bot).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .First(x => x.Key is "BuildStamp").Value!.Split(";");

    private static readonly string InformationalVersion
        = typeof(Bot).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
}
public static class KagamiBuildStamp
{
    //AssemblyVersionAttribute
    private static readonly string? _version
        = typeof(KagamiBuildStamp).Assembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version;
    private static readonly Dictionary<string, string> _list = typeof(KagamiBuildStamp)
        .Assembly
        .GetCustomAttributes<AssemblyMetadataAttribute>()
        .ToDictionary(
            i => i.Key,
            i => i.Value!);
    public static string Branch { get; } = _list[nameof(Branch)];

    public static string Revision { get; } = _list[nameof(Revision)];

    public static string? Version => _version;

    public static DateTime BuildTime { get; } = DateTime.Parse(_list[nameof(BuildTime)]);

}
