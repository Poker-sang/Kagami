using Konata.Core.Message;

namespace Kagami.Services;

public static class Bing
{
    private const string BaseUri = "https://cn.bing.com/";
    private const string Image = "HPImageArchive.aspx?n=1&format=js";

    public static async Task<MessageBuilder> PictureAsync()
    {
        var json = await (BaseUri + Image).DownloadJsonAsync();
        var obj = json.RootElement.GetProperty("images")[0];
        var uri = obj.GetProperty("url").GetString()!.TrimStart('/');
        return new MessageBuilder()
            .Image(await (BaseUri + uri).DownloadBytesAsync())
            .TextLine()
            .TextLine(obj.GetProperty("title").GetString()!)
            .TextLine()
            .TextLine(obj.GetProperty("copyright").GetString()!)
            .TextLine(BaseUri + uri);
    }
}
