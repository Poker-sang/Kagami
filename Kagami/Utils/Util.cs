using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Kagami.Utils;

public static class Util
{
    public static double Bytes2MiB(this long bytes, int round) => Math.Round(bytes / 1048576.0, round);

    /// <summary>
    /// Convert bv into av
    /// </summary>
    /// <param name="bvCode"></param>
    /// <returns></returns>
    public static string Bv2Av(this string bvCode)
    {
        const long xor = 177451812L;
        const long add = 100618342136696320L;
        const string table = "fZodR9XQDSUm21yCkr6" +
                             "zBqiveYah8bt4xsWpHn" +
                             "JE7jL5VG3guMTKNPAwcF";

        var sed = new byte[] { 9, 8, 1, 6, 2, 4, 0, 7, 3, 5 };
        var chars = new Dictionary<char, int>();
        for (var i = 0; i < table.Length; ++i)
            chars.Add(table[i], i);

        try
        {
            var r = 0L;
            for (var i = 0; i < sed.Length; i++)
                r += chars[bvCode[sed[i]]]
                     * (long)Math.Pow(table.Length, i);

            var result = r - add ^ xor;
            return result is > 10000000000 or < 0 ? "" : $"av{result}";
        }

        catch
        {
            return "";
        }
    }

    private const string CnNumber = "零一二三四五六七八九十百千万";

    /// <summary>
    /// 阿拉伯数字转中文数字
    /// </summary>
    /// <param name="integer"></param>
    /// <returns></returns>
    public static string NumberToCn(this int integer)
    {
        if (integer is 0)
            return "零";
        var cnNumber = new StringBuilder();
        for (var i = 0; integer is not 0; ++i)
        {
            var temp = integer % 10;
            if (temp is not 0)
            {
                cnNumber.Insert(0, i switch
                {
                    1 => "十",
                    2 => "百",
                    3 => "千",
                    4 => "万",
                    5 => "十万",
                    6 => "百万",
                    7 => "千万",
                    8 => "亿",
                    _ => ""
                });
                cnNumber.Insert(0, CnNumber[temp]);
            }
            else if (cnNumber.Length is not 0 && cnNumber[0] is not '零')
                cnNumber.Insert(0, "零");
            integer /= 10;
        }
        return cnNumber.ToString();
    }

    /// <summary>
    /// UrlDownload file
    /// </summary>
    /// <param name="url"></param>
    /// <param name="header"></param>
    /// <param name="timeout"></param>
    /// <param name="limitLen">default 2 gigabytes</param>
    /// <returns></returns>
    public static async Task<byte[]> UrlDownload(this string url, Dictionary<string, string>? header = null, int timeout = 8000, long limitLen = ((long)2 << 30) - 1)
    {
        // Create request
        var request = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.All })
        {
            Timeout = new TimeSpan(0, 0, 0, timeout),
            MaxResponseContentBufferSize = limitLen
        };
        // Default useragent
        request.DefaultRequestHeaders.Add("User-Agent", new[]
        {
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64)" ,
            "AppleWebKit/537.36 (KHTML, like Gecko)" ,
            "Chrome / 100.0.4896.127 Safari / 537.36 Edg / 100.0.1185.50" ,
            "Poker Kagami/1.0.0 (Konata Project)"
        });
        // Append request header
        if (header is not null)
            foreach (var (k, v) in header)
                request.DefaultRequestHeaders.Add(k, v);

        // Open response stream
        var response = await request.GetByteArrayAsync(url);

        // Receive the response data
        return response;
    }



    /// <summary>
    /// Get meta data
    /// </summary>
    /// <param name="keys"></param>
    /// <param name="html"></param>
    /// <returns></returns>
    public static Dictionary<string, string> GetMetaData(this string html, params string[] keys)
    {
        var metaDict = new Dictionary<string, string>();

        foreach (var i in keys)
        {
            var pattern = i + @"=""(.*?)""(.|\s)*?content=""(.*?)"".*?>";

            // Match results
            foreach (Match j in Regex.Matches(html, pattern, RegexOptions.Multiline))
                metaDict.TryAdd(j.Groups[1].Value, j.Groups[3].Value);
        }

        return metaDict;
    }

    /// <summary>
    /// Can I do
    /// </summary>
    /// <param name="factor">Probability scale</param>
    /// <returns></returns>
    public static bool CanIDo(double factor = 0.5f) => new Random().NextDouble() >= 1 - factor;
}