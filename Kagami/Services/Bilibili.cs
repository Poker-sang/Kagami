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
}