using System.Text.RegularExpressions;

namespace Kagami.Extensions;
internal static class HtmlExtensions
{
    /// <summary>
    /// Get meta data
    /// </summary>
    /// <param name="keys"></param>
    /// <param name="html"></param>
    /// <returns></returns>
    public static Dictionary<string, string> GetMetaData(this string html, params string[] keys)
    {
        var metaDict = new Dictionary<string, string>();

        foreach (var i in keys)
        {
            var pattern = i + @"=""(.*?)""(.|\s)*?content=""(.*?)"".*?>";

            // Match results
            foreach (var j in Regex.Matches(html, pattern, RegexOptions.Multiline).Cast<Match>())
                _ = metaDict.TryAdd(j.Groups[1].Value, j.Groups[3].Value);
        }

        return metaDict;
    }
}
