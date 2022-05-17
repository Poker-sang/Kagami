using Kagami.Utils;
using Konata.Core.Message;
using System.Text.Json;

namespace Kagami.Services;

public static class Bing
{
    private const string BaseUri = "https://cn.bing.com/";
    private const string Image = "HPImageArchive.aspx?n=1&format=js";

    public static async Task<MessageBuilder> PictureAsync()
    {
        JsonDocument json = await (BaseUri + Image).DownloadJsonAsync();
        var obj = json.RootElement.GetProperty("images")[0];
        string uri = obj.GetProperty("url").GetString()!.TrimStart('/');
        var data = await (BaseUri + uri).DownloadBytesAsync();
        return new MessageBuilder()
            .Image(data)
            .TextLine()
            .TextLine(obj.GetProperty("title").GetString()!)
            .TextLine()
            .TextLine(obj.GetProperty("copyright").GetString()!)
            .TextLine(BaseUri + uri);
    }
}
