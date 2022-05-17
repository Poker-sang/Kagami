using Kagami.Utils;
using Konata.Core.Message;
using System.Diagnostics;

namespace Kagami.Services;

/// <summary>
/// 和B站相关的业务逻辑代码
/// </summary>
public static class Bilibili
{
    /// <summary>
    /// 获取B站的视频基本信息
    /// </summary>
    /// <param name="code">aid/bvid</param>
    /// <returns></returns>
    public static async Task<MessageBuilder> GetVideoInfoFrom(string code)
    {
        var uri = $"https://www.bilibili.com/video/{code}";
        Debug.WriteLine($"[{nameof(Bilibili)}]::({nameof(GetVideoInfoFrom)}): Get From: \"{uri}\"");

        // UrlDownload the page
        var html = await uri.DownloadStringAsync();
        // Get meta data
        var metaData = html.GetMetaData("itemprop");
        var titleMeta = metaData["description"];
        var imageMeta = metaData["image"];
        // var keywordMeta = metaData["keywords"];

        // UrlDownload the image
        var image = await imageMeta.DownloadBytesAsync();

        // Build message
        return new MessageBuilder($"{titleMeta}")
            .TextLine(uri)
            .TextLine()
            .Image(image);
        // .TextLine("#" + string.Join(" #", keywordMeta.Split(",")[1..^4]));
    }

    /// <summary>
    /// Convert bv into av
    /// </summary>
    /// <param name="bvCode"></param>
    /// <returns></returns>
    public static string? Bv2Av(this string bvCode)
    {
        const long xor = 177451812L;
        const long add = 100618342136696320L;
        const string table = "fZodR9XQDSUm21yCkr6" +
                             "zBqiveYah8bt4xsWpHn" +
                             "JE7jL5VG3guMTKNPAwcF";

        var sed = new byte[] { 9, 8, 1, 6, 2, 4, 0, 7, 3, 5 };
        var chars = new Dictionary<char, int>();
        for (var i = 0; i < table.Length; ++i)
            chars.Add(table[i], i);

        try
        {
            var r = sed.Select((t, i) => chars[bvCode[t]] * (long)Math.Pow(table.Length, i)).Sum();

            var result = (r - add) ^ xor;
            return result is > 10000000000 or < 0 ? "" : result.ToString();
        }
        catch
        {
            return null;
        }
    }
}
