using Kagami.ArgTypes;
using Kagami.Attributes;
using Kagami.Utils;
using Konata.Core;
using Konata.Core.Common;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Konata.Core.Message.Model;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Kagami.Function;

public static partial class Commands
{
    [Help("相关数据")]
    private static MessageBuilder Status()
        => Text("[Poker Kagami] 内核信息")
            // Core descriptions
            .TextLine($"[分支:{BuildStamp.Branch}]")
            .TextLine($"[提交:{BuildStamp.CommitHash[..12]}]")
            .TextLine($"[版本:{BuildStamp.Version}]")
            .TextLine($"[{BuildStamp.BuildTime}]")
            .TextLine("Konata Project (C) 2022")
            .TextLine()
            // System status
            .TextLine($"处理了 {_messageCounter} 条消息")
            .TextLine($"GC内存 {GC.GetTotalAllocatedBytes().Bytes2MiB(2)} MiB ")
            .Text($"({Math.Round((double)GC.GetTotalAllocatedBytes() / GC.GetTotalMemory(false) * 100, 2)}%)")
            .TextLine($"总内存 {Process.GetCurrentProcess().WorkingSet64.Bytes2MiB(2)} MiB");

    [Help("看看我是否在线")]
    private static MessageBuilder Ping() => Text("Pong!");

    [Help("自我介绍")]
    private static MessageBuilder Greeting() => Text("你好！！！我是Poker Kagami");

    [Help("复读一句话", "话")]
    [CommandArgs(typeof(string))]
    private static MessageBuilder Repeat(TextChain text, MessageChain message) => Text(text.Content[6..].Trim()).Add(message[1..]);

    [Help("获取成员信息", "成员")]
    [CommandArgs(typeof(At))]
    private static async Task<MessageBuilder> Member(Bot bot, GroupMessageEvent group)
    {
        // Get at
        var at = group.Chain.FetchChain<AtChain>();
        if (at is null)
            return Text(ArgumentError);

        // Get group info
        var memberInfo = await bot.GetGroupMemberInfo(group.GroupUin, at.AtUin, true);
        if (memberInfo is null)
            return Text("没有找到这个人x");

        return Text($"[{memberInfo.NickName}]")
            .TextLine($"群名片：{memberInfo.Name}")
            .TextLine($"加入时间：{memberInfo.JoinTime}")
            .TextLine($"类别：{memberInfo.Role switch { RoleType.Member => "成员", RoleType.Admin => "管理员", RoleType.Owner => "群主", _ => throw new ArgumentOutOfRangeException() }}")
            .TextLine($"等级：{memberInfo.Level}")
            .TextLine($"头衔：{memberInfo.SpecialTitle}");
    }

    [Help("展示视频信息", "BV号")]
    [CommandArgs(typeof(string))]
    private static async Task<MessageBuilder> Bv(TextChain text)
    {
        if (text.Content[2..].Trim().Bv2Av() is not { } avCode)
            return Text("BV号不对哦");
        // UrlDownload the page
        var html = await $"https://www.bilibili.com/video/av{avCode}".UrlDownloadString();
        // Get meta data
        var metaData = html.GetMetaData("itemprop");
        var titleMeta = metaData["description"];
        var imageMeta = metaData["image"];
        // var keywordMeta = metaData["keywords"];

        // UrlDownload the image
        var image = await imageMeta.UrlDownloadBytes();

        // Build message
        return Text($"{titleMeta}")
            .TextLine($"https://www.bilibili.com/video/{avCode}")
            .TextLine()
            .Image(image);
        // .TextLine("#" + string.Join(" #", keywordMeta.Split(",")[1..^4]));
    }

    [Help("仓库信息", "组织名", "仓库名")]
    [CommandArgs(typeof(string), typeof(string))]
    private static async Task<MessageBuilder> GitHub(Bot bot, GroupMessageEvent group, TextChain text)
    {
        // UrlDownload the page
        try
        {
            if (text.Content[6..].Trim().Split(' ') is not { Length: 2 } args)
                return Text(ArgumentError);
            _ = await bot.SendGroupMessage(group.GroupUin, Text("获取仓库中..."));
            var html = await $"https://github.com/{args[0]}/{args[1]}.git".UrlDownloadString();
            // Get meta data
            var metaData = html.GetMetaData("property");
            var imageMeta = metaData["og:image"];

            // Build message
            var image = await imageMeta.UrlDownloadBytes();
            return new MessageBuilder().Image(image);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Not a repository link. \n{e.Message}");
            return Text("不是一个仓库链接");
        }
    }

    [Help("帮我选一个", "一些选项")]
    [CommandArgs(typeof(string[]))]
    private static MessageBuilder Roll(GroupMessageEvent group, TextChain text)
    {
        var items = text.Content[4..].Trim().Split(' ');
        return Text(items.Length < 2 ? "没有选项让我怎么选，笨！" : RollMessage.RandomGet().Replace("$", items.RandomGet()).Replace("扑克", group.MemberCard));
    }
    private static readonly string[] RollMessage = { "嗯让我想想ww......果然还是$好！", "emmm我想选$吧x", "要不还是选$呢？", "就你了！$！" };

    [Help("获取图片", "指令")]
    [CommandArgs(typeof(PicCommands))]
    private static async Task<MessageBuilder> Pic(TextChain text)
    {
        var content = text.Content[3..].Trim().ToLower().Split(' ');
        switch (content[0])
        {
            case "bing":
                {
                    var result = await "https://api.xygeng.cn/Bing/url".UrlDownloadString();
                    result = Regex.Match(result, @"""(http[^""]+)""").Groups[1].Value;
                    result = Regex.Unescape(result);
                    return new MessageBuilder().Image(await result.UrlDownloadBytes());
                }
        }

        return Text(ArgumentError);
    }
}