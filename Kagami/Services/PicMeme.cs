using Konata.Core.Message;
using System.Web;

namespace Kagami.Services;
internal static class PicMeme
{
    private const string baseUri = "https://gsapi.cyberrex.jp/image?top={0}&bottom={1}";

    private static long lastCalled = 0;

    private const int coolDown = 180;

    public static async Task<MessageBuilder> PictureAsync(string top, string bottom)
    {
        // 秒
        var time = DateTime.Now.Ticks / 10000000;
        if (time - lastCalled > coolDown)
        {
            lastCalled = time;
            top = Uri.EscapeDataString(top);
            bottom = Uri.EscapeDataString(bottom);
            return new MessageBuilder().Image(await string.Format(baseUri, top, bottom).DownloadBytesAsync());
        }
        else
            return new($"技能冷却中，还差{coolDown + lastCalled - time}秒");
    }
}
