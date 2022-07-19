using Konata.Core.Message;
using System.Diagnostics;

namespace Kagami.Services;

public static class AcFun
{
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
                var rawJson = line.Replace(flag, "").Trim().TrimEnd(';');
                var json = System.Text.Json.JsonDocument.Parse(rawJson).RootElement;
                if (json.TryGetProperty("title", out var element))
                    titleMeta = element.GetString() ?? "";
                if (json.TryGetProperty("description", out element))
                    descriptionMeta = element.GetString() ?? "";
                if (json.TryGetProperty("coverUrl", out element))
                    imageMeta = element.GetString() ?? "";
                break;
            }
        }

        // Build message
        return new MessageBuilder(titleMeta)
            .TextLine(descriptionMeta)
            .TextLine(uri)
            .TextLine()
            .Image(await imageMeta.DownloadBytesAsync());
    }
}
