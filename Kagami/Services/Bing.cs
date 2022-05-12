using System.Text.Json;
using Kagami.Utils;
using Konata.Core.Message;
using Konata.Core.Message.Model;

namespace Kagami.Services;

public static class Bing
{
    private const string BASE_URI = "https://cn.bing.com/";
    private const string IMAGE = "HPImageArchive.aspx?n=1&format=js";

    public static async Task<MessageBuilder> PictureAsync()
    {
        JsonDocument json = await (BASE_URI + IMAGE).DownloadJsonAsync();
        var obj = json.RootElement.GetProperty("images")[0];
        string uri = obj.GetProperty("url").GetString()!.TrimStart('/');
        var data = await (BASE_URI + uri).UrlDownloadBytes();
        return new MessageBuilder()
            .Image(data)
            .TextLine()
            .TextLine(obj.GetProperty("title").GetString()!)
            .TextLine()
            .TextLine(obj.GetProperty("copyright").GetString()!)
            .TextLine(BASE_URI + uri);
    }
}