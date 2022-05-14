using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Kagami.Services;

public static class Meme
{
    private const string UriTemplate = "https://cangku.icu/api/v1/post/search?search=沙雕图集锦 ";
    private const string Pattern = @"<img src=""([\w:/.]+)"" class=""[\w\- ]+"" alt=""[\w.]+"">";

    /// <summary>
    /// 图片存放总路径
    /// </summary>
    public const string MemePath = StringResources.EnvPath + @"memes\";
    /// <summary>
    /// 记录目前文件夹中最新一期期数的指针
    /// </summary>
    public const string NewPath = MemePath + "new.ptr";
    /// <summary>
    /// 记录现在已经发到第几张图片的指针
    /// </summary>
    public const string Pointer = "0.ptr";
    /// <summary>
    /// 记录某期所有图片链接的索引
    /// </summary>
    public const string Indexer = "1.idx";


    public static async Task<string[]> GetMemeImageSourcesAsync(string issue)
    {
        await Task.Yield();

        string uri = UriTemplate + $"第{issue}期";
        var json = await uri.DownloadJsonAsync();
        string content = json.RootElement.GetProperty("data")[0].GetProperty("content").GetString()!;
        // TODO: 404 NotFound
        return GetImageTags(content).ToArray();
    }

    /// <summary>
    /// 从RSS获取订阅
    /// </summary>
    /// <returns>图片链接和期数</returns>
    /// <exception cref="NotFoundException"></exception>
    public static async Task<(string[], string)> GetMemeImageSourcesRssAsync()
    {
        await Task.Yield();

        var xDocument = XDocument.Parse((await "https://cangku.icu/feed".DownloadStringAsync())[1..]);
        XNamespace d = "http://www.w3.org/2005/Atom";
        if (xDocument.Root is { } root)
            foreach (var entry in root.Descendants(d + "entry"))
            {
                // xElement.Element(d + "author")?.Element(d + "name")?.Value is not "錒嗄锕"
                if (entry.Element(d + "title")?.Value is { } entryTitle && entryTitle.Contains("沙雕图集锦"))
                {
                    var first = entryTitle.IndexOf('第');
                    var last = entryTitle.IndexOf('期');
                    if (first is not -1 && last is not -1 && entry.Element(d + "content") is { } content)
                    {
                        // 直接获取图片链接索引、期数
                        return (GetImageTags(content.Value).ToArray(), entryTitle[(first + 1)..last]);
                    }
                }
            }
        throw new KeyNotFoundException("RSS订阅失败！");
    }

    private static IEnumerable<string> GetImageTags(string content)
    {
        var matches = Regex.Matches(content, Pattern);
        var images = matches.Select(i => i.Groups[1].Value);
        return (content.Contains("语录") ? images.SkipLast(2) : images);
    }
}