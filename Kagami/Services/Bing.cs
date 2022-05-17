using System.Text.Json;

using Konata.Core.Message;

namespace Kagami.Services;

public static class Bing
{
    private const string BaseUri = "https://cn.bing.com/";
    private const string Image = "HPImageArchive.aspx?n=1&format=js";

    public static async Task<MessageBuilder> PictureAsync()
    {
        JsonDocument json = await (BaseUri + Image).DownloadJsonAsync();
        JsonElement obj = json.RootElement.GetProperty("images")[0];
        string uri = obj.GetProperty("url").GetString()!.TrimStart('/');
        byte[]? data = await (BaseUri + uri).DownloadBytesAsync();
        return new MessageBuilder()
            .Image(data)
            .TextLine()
            .TextLine(obj.GetProperty("title").GetString()!)
            .TextLine()
            .TextLine(obj.GetProperty("copyright").GetString()!)
            .TextLine(BaseUri + uri);
    }
}
