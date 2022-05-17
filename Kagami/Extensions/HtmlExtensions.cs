using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
            foreach (Match j in Regex.Matches(html, pattern, RegexOptions.Multiline))
                _ = metaDict.TryAdd(j.Groups[1].Value, j.Groups[3].Value);
        }

        return metaDict;
    }
}
