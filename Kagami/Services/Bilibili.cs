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

    private const int Xor = 177451812;

    private const long Add = 8728348608;

    private static ReadOnlySpan<byte> Table => "fZodR9XQDSUm21yCkr6zBqiveYah8bt4xsWpHnJE7jL5VG3guMTKNPAwcF"u8;

    public static unsafe string Av2Bv(this int av)
    {
        fixed (byte* table = Table)
        {
            var result = stackalloc byte[10] { 49, 0, 0, 52, 0, 49, 0, 55, 0, 0 };
            var temp = (av ^ Xor) + Add;
            result[9] = table[temp % 58];
            result[8] = table[temp / 58 % 58];
            result[1] = table[temp / (58 * 58) % 58];
            result[6] = table[temp / (58 * 58 * 58) % 58];
            result[2] = table[temp / (58 * 58 * 58 * 58) % 58];
            result[4] = table[temp / (58 * 58 * 58 * 58 * 58) % 58];
            return Encoding.ASCII.GetString(result, 12);
        }
    }

    public static int Bv2Av(this string bv) =>
        (int)((Table.IndexOf((byte)bv[9]) +
            Table.IndexOf((byte)bv[8]) * 58 +
            Table.IndexOf((byte)bv[1]) * 58 * 58 +
            Table.IndexOf((byte)bv[6]) * 58 * 58 * 58 +
            Table.IndexOf((byte)bv[2]) * 58 * 58 * 58 * 58 +
            Table.IndexOf((byte)bv[4]) * 58 * 58 * 58 * 58 * 58 - Add) ^ Xor);
}
