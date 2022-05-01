using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Kagami.Attributes;
using Kagami.Exceptions;
using Kagami.Utils;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Konata.Core.Message.Model;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

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
                divisions = driver.FindElements(By.XPath("/html/body/div/div[1]/div/div/div/div[3]/div[2]/span/div"));
                Thread.Sleep(1000);
            } while (divisions.Count is 0);
            var link = "";
            foreach (var division in divisions)
                if (division.FindElement(By.XPath("./div/section/a")) is { } anchor)
                    if (anchor.GetAttribute("title").Contains($"沙雕图集锦{issue}"))
                    {
                        link = anchor.GetAttribute("href");
                        break;
                    }

            driver.Url = link;

            ReadOnlyCollection<IWebElement>? images;
            do
            {
                images = driver.FindElements(By.XPath("/html/body/div/div[1]/div/div/div/div/div[1]/article/div[2]/p[1]/img"));
                Thread.Sleep(1000);
            } while (images.Count is 0);

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


    [Help("Update meme images")]
    private static async Task<MessageBuilder> Update(Bot bot, GroupMessageEvent group, TextChain text)
    {
        _ = await bot.SendGroupMessage(group.GroupUin, Text("Fetching meme images..."));
        try
        {
            var rst = Regex.Match(text.Content, @"\b[0-9]+\b");

            var directory = new DirectoryInfo(@$"C:\Users\poker\Desktop\{rst.Value}");
            string[] imgUrls;
            if (directory.Exists)
            {
                if (directory.GetFiles() is { Length: 1 } files && files[0] is { Name: "issue.txt" } txtFile)
                    imgUrls = await File.ReadAllLinesAsync(txtFile.FullName);
                else return Text("Meme images already existed!");
            }
            else
            {
                directory.Create();

                imgUrls = rst.Success
                    ? await GetMemeImageSources(int.Parse(rst.Value).NumberToCn())
                    : await GetMemeImageSources();

                await File.WriteAllLinesAsync(@$"{directory.FullName}\issue.txt", imgUrls);
            }

            foreach (var imgUrl in imgUrls)
                await File.WriteAllBytesAsync(@$"{directory.FullName}\{imgUrl.Split('/')[^1]}",
                    await imgUrl.UrlDownload());
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

}