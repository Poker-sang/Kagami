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
    /// https://cangku.icu/search/post?q=沙雕图集锦
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

            foreach (var paragraph in paragraphs)
                images.AddRange(paragraph.FindElements(By.TagName("img")));

            var array = images.Select(image => image.GetAttribute("src")).ToArray();
            return array;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            EdgeDriverManager.Quit();
        }
    }

    private const string SavePath = @"C:\Users\poker\Desktop\memes\";


    [Help("Update meme images")]
    private static async Task<MessageBuilder> UpdateMeme(Bot bot, GroupMessageEvent group, TextChain text)
    {
        _ = await bot.SendGroupMessage(group.GroupUin, Text("Fetching meme images..."));
        try
        {
            var result = Regex.Match(text.Content, @"\b[0-9]+\b");

            if (!result.Success)
                return Text("Need an argument");

            var directory = new DirectoryInfo(SavePath + result.Value);
            string[] imgUrls;
            if (directory.Exists)
            {
                if (directory.GetFiles() is { Length: 2 } files && files[1] is { Name: "1.txt" } txtFile)
                    imgUrls = await File.ReadAllLinesAsync(txtFile.FullName);
                else return Text("Meme images already existed!");
            }
            else
            {
                directory.Create();
                await File.WriteAllTextAsync(Path.Combine(directory.FullName, "0.bin"), 0.ToString());

                imgUrls = await GetMemeImageSources(int.Parse(result.Value).NumberToCn());

                await File.WriteAllLinesAsync(Path.Combine(directory.FullName, "1.txt"), imgUrls);
            }

            for (var i = 0; i < imgUrls.Length; i++)
                await File.WriteAllBytesAsync(Path.Combine(directory.FullName, (i + 2).ToString()),
                    await imgUrls[i].UrlDownload());

            return Text("Meme images updated!");
        }
        catch (EdgeDriverBusyException e)
        {
            Console.WriteLine(e);
            return Text("EdgeDriver is busy! You should not make two requests at the same time.");
        }
        catch (FormatException e)
        {
            Console.WriteLine(e);
            return Text("Bad argument");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Text("Meme images update failed! You can retry to request.");
        }
    }

    [Help("Send a meme image in sequence")]
    private static async Task<MessageBuilder> Meme(TextChain text)
    {
        try
        {
            var result = Regex.Match(text.Content, @"\b[0-9]+\b");

            var directory = !result.Success ? new DirectoryInfo(SavePath).GetDirectories()[^1] : new DirectoryInfo(SavePath + result.Value);

            var number = int.Parse(await File.ReadAllTextAsync(Path.Combine(directory.FullName, "0.bin")));
            var files = directory.GetFiles();
            if (number >= files.Length - 2)
                number = 0;

            var image = await File.ReadAllBytesAsync(Path.Combine(directory.FullName, (number + 2).ToString()));

            ++number;
            await File.WriteAllTextAsync(Path.Combine(directory.FullName, "0.bin"), number.ToString());

            var message = new MessageBuilder();
            message.Text($"{number}/{files.Length - 2}");
            message.Image(image);
            return message;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Text("Bad argument");
        }
    }
}