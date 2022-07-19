using Kagami.Core;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Kagami.Services;

public static class Meme
{
    private const string UriTemplate = "https://cangku.icu/api/v1/post/search?search=";
    private const string SearchTemplate = "沙雕图集锦 第{0}期";
    private const string Pattern = @"<img src=""([\w:/.]+)"" class=""[\w\- ]+"" alt=""[\w.]+"">";

    /// <summary>
    /// 图片存放总路径
    /// </summary>
    private const string MemePath = Paths.UtilitiesPath + "memes";

    /// <summary>
    /// 记录目前文件夹中最新一期期数的指针
    /// </summary>
    private const string NewPath = MemePath + "/new.ptr";

    /// <summary>
    /// 记录现在已经发到第几张图片的指针
    /// </summary>
    private const string Pointer = "0.ptr";

    /// <summary>
    /// 记录某期所有图片链接的索引
    /// </summary>
    private const string Indexer = "1.idx";

    public static bool IsRepoEmpty => !File.Exists(NewPath);

    public static async Task<string[]?> GetMemeImageSourcesAsync(int issue)
    {
        await Task.Yield();

        var target = string.Format(SearchTemplate, issue.IntToCn());
        var json = await (UriTemplate + target).DownloadJsonAsync();
        foreach (var datum in json.RootElement.GetProperty("data").EnumerateArray())
            if (datum.GetProperty("title").GetString() is { } title && title.Contains(target))
                if (datum.GetProperty("content").GetString() is { } content)
                    return GetImageTags(content).ToArray();

        // 尝试阿拉伯数字搜索
        target = string.Format(SearchTemplate, issue.ToString());
        json = await (UriTemplate + target).DownloadJsonAsync();
        foreach (var datum in json.RootElement.GetProperty("data").EnumerateArray())
            if (datum.GetProperty("title").GetString() is { } title && title.Contains(target))
                if (datum.GetProperty("content").GetString() is { } content)
                    return GetImageTags(content).ToArray();

        return null;
    }

    /// <summary>
    /// 从RSS获取订阅
    /// </summary>
    /// <returns>图片链接和期数</returns>
    private static async Task<(string[] ImgUrls, string Issue)> GetMemeImageSourcesRssAsync()
    {
        await Task.Yield();

        var xDocument = XDocument.Parse((await "https://cangku.icu/feed".DownloadStringAsync())[1..]);
        var d = XNamespace.Get("http://www.w3.org/2005/Atom");
        if (xDocument.Root is { } root)
            foreach (var entry in root.Descendants(d + "entry"))
                // xElement.Element(d + "author")?.Element(d + "name")?.Value is not "錒嗄锕"
                if (entry.Element(d + "title")?.Value is { } entryTitle && entryTitle.Contains("沙雕图集锦"))
                {
                    var first = entryTitle.IndexOf('第');
                    var last = entryTitle.IndexOf('期');
                    if (first is not -1 && last is not -1 && entry.Element(d + "content") is { } content)
                        // 直接获取图片链接索引、期数
                        return (GetImageTags(content.Value).ToArray(), entryTitle[(first + 1)..last]);
                }

        throw new KeyNotFoundException("RSS订阅失败！");
    }

    private static IEnumerable<string> GetImageTags(string content)
    {
        var matches = Regex.Matches(content, Pattern);
        var images = matches.Select(i => i.Groups[1].Value);
        return content.Contains("语录") ? images.SkipLast(2) : images;
    }

    public static async Task<string> GetNewestIssue() =>
        IsRepoEmpty ? throw new("仓库里还没有弔图，先更新吧x") : await File.ReadAllTextAsync(NewPath);

    /// <summary>
    /// 发送沙雕图
    /// </summary>
    /// <param name="issue">期数</param>
    /// <returns></returns>
    public static async Task<(string Result, string? Image)> GetMemePicAsync(string issue)
    {
        try
        {
            var directory = new DirectoryInfo(Path.Combine(MemePath, issue));

            // 指定期数不存在
            if (!directory.Exists)
                return new($"第{issue}期不存在", null);

            var pointer = uint.Parse(await File.ReadAllTextAsync(Path.Combine(directory.FullName, Pointer)));
            var files = directory.GetFiles().Length - 2;
            if (pointer >= files)
                pointer = 0;

            var image = Path.Combine(directory.FullName, (pointer + 2).ToString());

            ++pointer;
            await File.WriteAllTextAsync(Path.Combine(directory.FullName, Pointer), pointer.ToString());

            return ($"{issue} {pointer}/{files}", image);
        }
        catch (FormatException e)
        {
            Console.WriteLine(e);
            return new(StringResources.ArgumentErrorMessage.RandomGet(), null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new(e.Message, null);
        }
    }

    /// <summary>
    /// 发送沙雕图列表
    /// </summary>
    /// <returns></returns>
    public static string GetMemeList()
        => new DirectoryInfo(MemePath).GetDirectories().Aggregate("弔图已有期数：",
            (current, directoryInfo) => current + directoryInfo.Name + ", ")[..^2];

    public static (string Result, string[]? Images) GetAllMeme(string issue)
    {
        try
        {
            var directory = new DirectoryInfo(Path.Combine(MemePath, issue));

            // 指定期数不存在
            if (!directory.Exists)
                return new($"第{issue}期不存在", null);

            var images = directory.GetFiles()[2..].Select(fi => fi.FullName).ToArray();

            return ($"弔图：第{issue}期\n图源：cangku.icu", images);
        }
        catch (FormatException e)
        {
            Console.WriteLine(e);
            return new(StringResources.ArgumentErrorMessage.RandomGet(), null);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new(e.Message, null);
        }
    }

    /// <summary>
    /// 更新沙雕图
    /// </summary>
    /// <param name="issue">中文期数</param>
    /// <param name="intIssue">数字期数</param>
    /// <returns></returns>
    public static async Task<string> UpdateMemeAsync(string? issue = null, int intIssue = -1)
    {
        try
        {
            string[] imgUrls;
            string issuePath;
            // 未指定期数
            if (issue is null || intIssue is -1)
            {
                // 未指定期数，RSS获取订阅
                (imgUrls, var cnIssue) = await GetMemeImageSourcesRssAsync();

                var tryParse = cnIssue.CnToInt();
                issue = tryParse is -1 ? cnIssue : tryParse.ToString();
                issuePath = Path.Combine(MemePath, issue);
                if (!int.TryParse(issue, out intIssue))
                {
                    Console.WriteLine("Meme int parse failed!");
                    return StringResources.ArgumentErrorMessage.RandomGet();
                }

                // 记录下载的图片
                await DownloadMemesAsync(issuePath, imgUrls);
            }
            // 指定期数
            else
            {
                issuePath = Path.Combine(MemePath, issue);
                // 下载图片
                imgUrls = await DownloadMemesAsync(issuePath, intIssue);
            }

            // 获取图片
            for (var i = 0; i < imgUrls.Length; ++i)
                await File.WriteAllBytesAsync(Path.Combine(issuePath, (i + 2).ToString()),
                    await imgUrls[i].DownloadBytesAsync());

            // 若是最新的则写入总索引
            if (IsRepoEmpty || int.Parse(await File.ReadAllTextAsync(NewPath)) < intIssue)
                await File.WriteAllTextAsync(NewPath, issue);
        }
        catch (OperationCanceledException)
        {
            return $"{issue}期弔图已存在！";
        }
        catch (KeyNotFoundException)
        {
            return $"未找到{issue}期弔图！";
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine(e);
            return e.Message;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return "弔图更新失败！你可以重新尝试";
        }

        return $"弔图已更新！第{issue}期";
    }

    /// <summary>
    /// 记录下载的图片
    /// </summary>
    /// <param name="issuePath">期数所在路径</param>
    /// <param name="imgUrls">图片链接索引</param>
    /// <returns></returns>
    /// <exception cref="OperationCanceledException">已存在</exception>
    private static async Task DownloadMemesAsync(string issuePath, string[] imgUrls)
    {
        // 如果已经有文件夹且索引和图片都有
        if (Directory.Exists(issuePath))
        {
            if (Directory.GetFiles(issuePath) is not { Length: 2 } files ||
                files[1] != Path.Combine(issuePath, Indexer))
                throw new OperationCanceledException();
        }
        // 没有文件夹
        else
        {
            // 记录索引和指针
            _ = Directory.CreateDirectory(issuePath);
            await File.WriteAllTextAsync(Path.Combine(issuePath, Pointer), "0");
            await File.WriteAllLinesAsync(Path.Combine(issuePath, Indexer), imgUrls);
        }
    }

    /// <summary>
    /// 获取imgUrls
    /// </summary>
    /// <returns></returns>
    /// <exception cref="OperationCanceledException">已存在</exception>
    private static async Task<string[]> DownloadMemesAsync(string issuePath, int intIssue)
    {
        string[] imgUrls;
        // 如果已经有文件夹
        if (Directory.Exists(issuePath))
        {
            imgUrls = Directory.GetFiles(issuePath) is { Length: 2 } files &&
                files[1] == Path.Combine(issuePath, Indexer)
                // 只有索引
                ? await File.ReadAllLinesAsync(files[1])
                // 索引和图片都有
                : throw new OperationCanceledException();
        }
        // 没有文件夹
        else
        {
            // 指定期数时，需要下载图片链接索引
            // may throw FileNotFoundException
            imgUrls = await GetMemeImageSourcesAsync(intIssue) ?? throw new KeyNotFoundException();
            // 记录索引和指针
            _ = Directory.CreateDirectory(issuePath);
            await File.WriteAllTextAsync(Path.Combine(issuePath, Pointer), "0");
            await File.WriteAllLinesAsync(Path.Combine(issuePath, Indexer), imgUrls);
        }

        return imgUrls;
    }
}
