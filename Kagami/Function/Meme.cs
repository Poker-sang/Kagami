using Kagami.Attributes;
using Kagami.Exceptions;
using Kagami.Utils;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Konata.Core.Message.Model;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Kagami.Function;

public static partial class Commands
{
    /// <summary>
    /// 动态获取网页的图片
    /// </summary>
    /// <param name="issue">期数</param>
    /// <returns></returns>
    /// https://cangku.icu/search/post?q=沙雕图集锦
    /// <div class="search-post">
    ///   <span class="row">
    ///   <div class="post col-sm-6 col-md-4">
    ///    <div class="post-card thumb-post-card">
    ///     <section class="post-card-wrap">
    ///      <a href = "/archives/[0-9]+" class="" title="[图片分享] 沙雕图集锦 第[汉字数字]+期 [汉字]+" target=""/>
    ///      ......
    ///     </section>
    ///    </div>
    ///   </div>
    ///   <div class="post col-sm-6 col-md-4"> ...... </div>
    ///   <div class="post col-sm-6 col-md-4"> ...... </div>
    ///  </span>
    /// </div>
    /// ......
    /// https://cangku.icu/search/post?q=沙雕图集锦 第xxx期
    private static async Task<string[]> GetMemeImageSources(string? issue = null)
    {
        await Task.Yield();

        var driver = EdgeDriverManager.GetEdgeDriver(5);
        try
        {
            issue = issue is null ? "" : $" 第{issue}期";
            driver.Url = $"https://cangku.icu/search/post?q=沙雕图集锦{issue}";

            ReadOnlyCollection<IWebElement>? divisions;
            do
            {
                // 非常抱歉，找不到匹配的内容
                if (driver.TryFindElement(By.XPath("/html/body/div/div[1]/div/div/div/div[3]/div[2]/div"))?.Text is "抱歉")
                    throw new NotFoundException("Issue not found!");
                divisions = driver.FindElements(By.XPath("/html/body/div/div[1]/div/div/div/div[3]/div[2]/span/child::*"));
                Thread.Sleep(1000);
            } while (divisions is { Count: 0 });
            var link = "";
            foreach (var division in divisions)
                if (division.FindElement(By.XPath("./div/section/a")) is { } anchor)
                    if (anchor.GetAttribute("title").Contains($"沙雕图集锦{issue}"))
                    {
                        link = anchor.GetAttribute("href");
                        break;
                    }

            driver.Url = link;

            ReadOnlyCollection<IWebElement>? paragraphs;
            do
            {
                paragraphs = driver.FindElements(By.XPath("/html/body/div/div[1]/div/div/div/div/div[1]/article/div[2]/child::*"));
                Thread.Sleep(1000);
            } while (paragraphs.Count is 0);

            var images = new List<IWebElement>();

            foreach (var paragraph in paragraphs.SkipLast(1))
                images.AddRange(paragraph.FindElements(By.TagName("img")));
            // 今日喵喵语录两则
            if (!paragraphs[^1].Text.Contains("语录"))
                images.AddRange(paragraphs[^1].FindElements(By.TagName("img")));

            var array = images.Select(image => image.GetAttribute("src")).ToArray();
            return array;
        }
        finally
        {
            EdgeDriverManager.Quit();
        }
    }

    /// <summary>
    /// 从RSS获取订阅
    /// </summary>
    /// <returns>图片链接和期数</returns>
    /// <exception cref="NotFoundException"></exception>
    private static async Task<(string[], string)> GetMemeImageSourcesRss()
    {
        await Task.Yield();

        var xDocument = XDocument.Parse((await "https://cangku.icu/feed".UrlDownloadString())[1..]);
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
                        var result = Regex.Matches(content.Value, @"src=""([^""]+)""")
                            .Select(match => match.Groups[1].Value);
                        // 直接获取图片链接索引、期数
                        return ((content.Value.Contains("语录") ? result.SkipLast(2) : result).ToArray(), entryTitle[(first + 1)..last]);
                    }
                }
            }
        throw new NotFoundException("RSS subscription failed!");
    }


    /// <summary>
    /// 图片存放总路径
    /// </summary>
    private const string SavePath = @"C:\Users\poker\Desktop\memes\";
    /// <summary>
    /// 记录现在已经发到第几张图片的指针
    /// </summary>
    private const string Pointer = "0.ptr";
    /// <summary>
    /// 记录某期所有图片链接的索引
    /// </summary>
    private const string Indexer = "1.idx";
    /// <summary>
    /// 记录目前文件夹中最新一期期数的指针
    /// </summary>
    private const string NewPath = SavePath + "new.ptr";


    [Help("[-Command][-Issue] Send a meme image in sequence")]
    private static async Task<MessageBuilder> Meme(Bot bot, GroupMessageEvent group, TextChain text)
    {
        var content = text.Content[4..].Trim().ToLower().Split(' ');
        switch (content[0])
        {
            // 更新图片
            case "update":
                {
                    try
                    {
                        _ = await bot.SendGroupMessage(group.GroupUin, Text("Fetching meme images..."));
                        // 图片链接索引
                        string[]? imgUrls = null;
                        // 期数的汉字数字字符串
                        string? cnIssue = null;
                        // 期数的阿拉伯数字字符串
                        string? issue;
                        // 未指定期数，RSS获取订阅
                        if (content.Length is 1)
                            try
                            {
                                (imgUrls, cnIssue) = await GetMemeImageSourcesRss();
                                issue = cnIssue.CnToInt().ToString();
                            }
                            catch (NotFoundException e)
                            {
                                Console.WriteLine(e);
                                return Text(e.Message);
                            }
                        // 指定期数
                        // var result = Regex.Match(content[0], @"\b[0-9]+\b");
                        else issue = content[1];
                        // 下载图片
                        var directory = new DirectoryInfo(SavePath + issue);
                        try
                        {
                            // 如果已经有文件夹
                            if (directory.Exists)
                            {
                                // 只有索引
                                if (directory.GetFiles() is { Length: 2 } files &&
                                    files[1] is { Name: Indexer } txtFile)
                                    imgUrls ??= await File.ReadAllLinesAsync(txtFile.FullName);
                                // 索引和图片都有
                                else return Text("Meme images already existed!");
                            }
                            // 没有文件夹
                            else
                            {
                                // may throw FormatException
                                var newInt = int.Parse(issue);
                                // 指定期数时，需要阿拉伯数字转汉字
                                cnIssue ??= newInt.IntToCn();
                                // 指定期数时，需要下载图片链接索引
                                // may throw EdgeDriverBusyException, NotFoundException
                                imgUrls ??= await GetMemeImageSources(cnIssue);
                                directory.Create();
                                // 记录索引和指针
                                await File.WriteAllTextAsync(Path.Combine(directory.FullName, Pointer), 0.ToString());
                                await File.WriteAllLinesAsync(Path.Combine(directory.FullName, Indexer), imgUrls);
                                // 若已有最新的则不写入总索引
                                if (int.Parse(await File.ReadAllTextAsync(NewPath)) < newInt)
                                    await File.WriteAllTextAsync(NewPath, issue);
                            }
                        }
                        catch (EdgeDriverBusyException e)
                        {
                            Console.WriteLine(e);
                            return Text(e.Message);
                        }
                        catch (NotFoundException e)
                        {
                            Console.WriteLine(e);
                            return Text(e.Message);
                        }
                        catch (FormatException e)
                        {
                            Console.WriteLine(e);
                            return Text("Bad argument, needs an integer.");
                        }
                        // 获取图片
                        for (var i = 0; i < imgUrls.Length; ++i)
                            await File.WriteAllBytesAsync(Path.Combine(directory.FullName, (i + 2).ToString()),
                                await imgUrls[i].UrlDownloadBytes());

                        return Text($"Meme images updated! Issue: No.{issue}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        return Text("Meme images update failed! May you retry to request.");
                    }
                }
            // 发送图片
            default:
                try
                {
                    if (!File.Exists(NewPath))
                        return Text("No meme images yet.");

                    var issue = content[0] is ""
                        // 未指定期数
                        ? await File.ReadAllTextAsync(NewPath)
                        // 指定期数
                        // var result = Regex.Match(content[0], @"\b[0-9]+\b");
                        : content[0];

                    var directory = new DirectoryInfo(SavePath + issue);

                    // 指定期数不存在
                    if (!directory.Exists)
                        return Text("Issue not exists");

                    var pointer = int.Parse(await File.ReadAllTextAsync(Path.Combine(directory.FullName, Pointer)));
                    var files = directory.GetFiles();
                    if (pointer >= files.Length - 2)
                        pointer = 0;

                    var image = await File.ReadAllBytesAsync(Path.Combine(directory.FullName,
                        (pointer + 2).ToString()));

                    ++pointer;
                    await File.WriteAllTextAsync(Path.Combine(directory.FullName, Pointer), pointer.ToString());

                    var message = new MessageBuilder();
                    message.Text($"{issue} {pointer}/{files.Length - 2}");
                    message.Image(image);
                    return message;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return Text("Bad argument, needs an integer.");
                }
        }
    }
}