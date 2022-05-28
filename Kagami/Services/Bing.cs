using Konata.Core.Message;

namespace Kagami.Services;

public static class Bing
{
    private const string baseUri = "https://cn.bing.com/";
    private const string image = "HPImageArchive.aspx?n=1&format=js";

    public static async Task<MessageBuilder> PictureAsync()
    {
        var json = await (baseUri + image).DownloadJsonAsync();
        var obj = json.RootElement.GetProperty("images")[0];
        var uri = obj.GetProperty("url").GetString()!.TrimStart('/');
        var data = await (baseUri + uri).DownloadBytesAsync();
        return new MessageBuilder()
            .Image(data)
            .TextLine()
            .TextLine(obj.GetProperty("title").GetString()!)
            .TextLine()
            .TextLine(obj.GetProperty("copyright").GetString()!)
            .TextLine(baseUri + uri);
    }
}
