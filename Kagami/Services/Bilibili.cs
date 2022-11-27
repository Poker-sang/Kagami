using Konata.Core.Message;
using System.Diagnostics;
using System.Text;

namespace Kagami.Services;

public static class Bilibili
{
    public static async Task<MessageBuilder> GetVideoInfoFrom(string code)
    {
        var uri = $"https://www.bilibili.com/video/{code}";
        Debug.WriteLine($"[{nameof(Bilibili)}]::({nameof(GetVideoInfoFrom)}): Get From: \"{uri}\"");

        // UrlDownload the page
        var html = await uri.DownloadStringAsync();
        // Get meta data
        var metaData = html.GetMetaData("itemprop");
        var name = metaData["name"];
        var description = metaData["description"];
        var imageMeta = metaData["image"];
        // var keywordMeta = metaData["keywords"];

        // Build message
        return new MessageBuilder(name)
            .TextLine(description)
            .TextLine(uri)
            .TextLine()
            .Image(await imageMeta.DownloadBytesAsync());
        // .TextLine("#" + string.Join(" #", keywordMeta.Split(",")[1..^4]));
    }

    /// <summary>
    /// Length = 58
    /// </summary>
    private const string Table = "fZodR9XQDSUm21yCkr6zBqiveYah8bt4xsWpHnJE7jL5VG3guMTKNPAwcF";
    private const long Xor = 177451812;
    private const long Add = 8728348608L;

    private static ulong IntPow(uint a, uint b)
    {
        var power = 1UL;
        for (var i = 0; i < b; ++i)
            power *= a;
        return power;
    }

    public static string Av2Bv(this ulong avCode)
    {
        try
        {
            avCode = (avCode ^ Xor) + Add;

            return new StringBuilder("1")
                .Append(Table[(int)(avCode / (58 * 58) % 58)])
                .Append(Table[(int)(avCode / (58 * 58 * 58 * 58)) % 58])
                .Append('4')
                .Append(Table[(int)(avCode / (58 * 58 * 58 * 58 * 58)) % 58])
                .Append('1')
                .Append(Table[(int)(avCode / (58 * 58 * 58)) % 58])
                .Append('7')
                .Append(Table[(int)(avCode / 58 % 58)])
                .Append(Table[(int)(avCode % 58)])
                .ToString();
        }
        catch
        {
            return "Error";
        }
    }

    public static unsafe ulong Bv2Av(this string bvCode)
    {
        var sed = stackalloc byte[10] { 9, 8, 1, 6, 2, 4, 0, 7, 3, 5 };
        var chars = new Dictionary<char, uint>();
        for (var i = 0U; i < 58; ++i)
            chars.Add(Table[(int)i], i);

        try
        {
            var result = 0UL;
            for (var i = 0U; i < 6; ++i)
                result += chars[bvCode[sed[i]]] * IntPow(58, i);
            return result - Add ^ Xor;
        }
        catch
        {
            return 0;
        }
    }
}
