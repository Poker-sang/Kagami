using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace Kagami.Function;

public static partial class Commands
{
    /// <summary>
    /// 动态获取网页的图片
    /// </summary>
    /// <param name="html"></param>
    /// <param name="pseudoClass"></param>
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
    private static async Task<IEnumerable<string>> GetImageSources(this string html, string pseudoClass)
    {
        await Task.Yield();

        var driver = EdgeDriverManager.GetEdgeDriver(5);
        try
        {
            driver.Navigate().GoToUrl("https://cangku.icu/search/post?q=沙雕图集锦");

            var divisions = driver.FindElements(By.XPath("/html/body/div/div[1]/div/div/div/div[3]/div[2]/span/div"));
            var link = "";
            foreach (var division in divisions)
                if (division.FindElement(By.XPath("./div/section/a")) is { } anchor)
                    if (anchor.GetAttribute("title").Contains("沙雕图集锦"))
                    {
                        link = anchor.GetAttribute("href");
                        break;
                    }

            driver.Navigate().GoToUrl(link);

            var images =
                driver.FindElements(By.XPath("/html/body/div/div[1]/div/div/div/div/div[1]/article/div[2]/p[1]/img"));

            var list = images.Select(image => image.GetAttribute("src")).ToList();
            return list;
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
    private static async Task<MessageBuilder> Update(Bot bot, GroupMessageEvent group, TextChain chain)
    {
        _ = await bot.SendGroupMessage(group.GroupUin, Text("Fetching meme images..."));
        try
        {
            var imgUrls = await GetImageSources(null, null);

            foreach (var imgUrl in imgUrls)
                await File.WriteAllBytesAsync(@$"C:\Users\poker\Desktop\123\{imgUrl}", await imgUrl.UrlDownload());

            return Text("Meme images updated!");
        }
        catch (EdgeDriverBusyException e)
        {
            Console.WriteLine(e);
            return Text(e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Text("Meme images update failed!");
        }
    }

}