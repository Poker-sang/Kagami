using Konata.Core.Message;
using System.Diagnostics;

namespace Kagami.Services;

/// <summary>
/// 和A站相关的业务逻辑代码
/// </summary>
public static class AcFun
{
    /// <summary>
    /// 获取A站的视频基本信息
    /// </summary>
    /// <param name="code">id</param>
    /// <returns></returns>
    public static async Task<MessageBuilder> GetVideoInfoFrom(string code)
    {
        var uri = $"https://www.acfun.cn/v/{code}";
        Debug.WriteLine($"[{nameof(AcFun)}]::({nameof(GetVideoInfoFrom)}): Get From: \"{uri}\"");

        // UrlDownload the page
        var html = await uri.DownloadStringAsync();
        // Get meta data
        var titleMeta = "";
        var imageMeta = "";
        var descriptionMeta = "";

        const string flag = "window.pageInfo = window.videoInfo = ";
        var sr = new StringReader(html);
        while (sr.Peek() >= 0)
        {
            var line = await sr.ReadLineAsync();
            if (line?.Contains(flag) ?? false)
            {
                var raw_json = line.Replace(flag, "").Trim().TrimEnd(';');
                var json = System.Text.Json.JsonDocument.Parse(raw_json).RootElement;
                if (json.TryGetProperty("title", out var element))
                    titleMeta = element.GetString() ?? "";
                if (json.TryGetProperty("description", out element))
                    descriptionMeta = element.GetString() ?? "";
                if (json.TryGetProperty("coverUrl", out element))
                    imageMeta = element.GetString() ?? "";
                break;
            }
        }

        // UrlDownload the image
        var image = await imageMeta.DownloadBytesAsync();

        // Build message
        return new MessageBuilder($"{titleMeta}")
            .TextLine(descriptionMeta)
            .TextLine(uri)
            .TextLine()
            .Image(image);
    }
}