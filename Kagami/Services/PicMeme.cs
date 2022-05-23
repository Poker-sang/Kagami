using Konata.Core.Message;
using System.Web;

namespace Kagami.Services;
internal static class PicMeme
{
    private const string BaseUri = "https://gsapi.cyberrex.jp/image?top={0}&bottom={1}";

    private static long LastCalled = 0;

    private const int CoolDown = 180;

    public static async Task<MessageBuilder> PictureAsync(string top, string bottom)
    {
        // 秒
        var time = DateTime.Now.Ticks / 10000000;
        if (time - LastCalled > CoolDown)
        {
            LastCalled = time;
            top = HttpUtility.UrlEncode(top);
            bottom = HttpUtility.UrlEncode(bottom);
            return new MessageBuilder().Image(await string.Format(BaseUri, top, bottom).DownloadBytesAsync());
        }
        else
            return new($"技能冷却中，还差{CoolDown + LastCalled - time}秒");
    }
}
