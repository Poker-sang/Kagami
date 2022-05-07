using Kagami.Attributes;
using Kagami.Utils;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Exceptions.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Konata.Core.Message.Model;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Kagami.ArgTypes;

namespace Kagami.Function;

public static partial class Commands
{
    [Help("相关数据")]
    private static MessageBuilder Status()
        => Text("[Poker Kagami]")
               // Core descriptions
               .TextLine($"[分支:{BuildStamp.Branch}]")
               .TextLine($"[提交:{BuildStamp.CommitHash[..12]}]")
               .TextLine($"[版本:{BuildStamp.Version}]")
               .TextLine($"[{BuildStamp.BuildTime}]")
               .TextLine()
               // System status
               .TextLine($"处理了 {_messageCounter} 条消息")
               .TextLine($"GC内存 {GC.GetTotalAllocatedBytes().Bytes2MiB(2)} MiB ")
               .Text($"({Math.Round((double)GC.GetTotalAllocatedBytes() / GC.GetTotalMemory(false) * 100, 2)}%)")
               .TextLine($"总内存 {Process.GetCurrentProcess().WorkingSet64.Bytes2MiB(2)} MiB")
               .TextLine()
               // Copyrights
               .TextLine("Konata Project (C) 2022");

    [Help("打招呼")]
    private static MessageBuilder Greeting() => Text("你好！！！我是Poker Kagami");

    [Help("复读一句话", "message")]
    [HelpArgs(typeof(string))]
    private static MessageBuilder Repeat(TextChain text, MessageChain message) => Text(text.Content[4..].Trim()).Add(message[1..]);

    [Help("获取成员信息", "member")]
    [HelpArgs(typeof(At))]
    private static async Task<MessageBuilder> Member(Bot bot, GroupMessageEvent group)
    {
        // Get at
        var at = group.Chain.GetChain<AtChain>();
        if (at is null)
            return Text(ArgumentError);

        // Get group info
        var memberInfo = await bot.GetGroupMemberInfo(group.GroupUin, at.AtUin, true);
        if (memberInfo is null)
            return Text("没有找到这个人x");

        return Text("[成员信息]")
            .TextLine($"名称: {memberInfo.Name}")
            .TextLine($"加入时间: {memberInfo.JoinTime}")
            .TextLine($"类别: {memberInfo.Role}")
            .TextLine($"等级: {memberInfo.Level}")
            .TextLine($"头衔: {memberInfo.SpecialTitle}")
            .TextLine($"昵称: {memberInfo.NickName}");
    }

    [Help("禁言一个人", "member", "minute")]
    [HelpArgs(typeof(At), typeof(uint?))]
    private static async Task<MessageBuilder> Mute(Bot bot, GroupMessageEvent group)
    {
        // Get at
        var atChain = group.Chain.GetChain<AtChain>();
        if (atChain is null)
            return Text(ArgumentError);

        var time = 10U;
        var textChains = group.Chain.FindChain<TextChain>();
        // Parse time
        if (textChains.Count is 2 &&
            uint.TryParse(textChains[1].Content, out var t))
            time = t;

        try
        {
            if (await bot.GroupMuteMember(group.GroupUin, atChain.AtUin, time * 60))
                return Text($"禁言 [{atChain.AtUin}] {time}分钟");
            return Text(UnknownError);
        }
        catch (OperationFailedException e)
        {
            Console.WriteLine(e.Message);
            return Text(OperationFailed);
        }
    }

    [Help("设置头衔", "member", "title")]
    [HelpArgs(typeof(At), typeof(string))]
    private static async Task<MessageBuilder> Title(Bot bot, GroupMessageEvent group)
    {
        // Get at
        var atChain = group.Chain.GetChain<AtChain>();
        if (atChain is null)
            return Text(ArgumentError);

        var textChains = group.Chain.FindChain<TextChain>();
        // Check argument
        if (textChains.Count is not 2)
            return Text(ArgumentError);

        try
        {
            if (await bot.GroupSetSpecialTitle(group.GroupUin, atChain.AtUin, textChains[1].Content, uint.MaxValue))
                return Text($"为 [{atChain.AtUin}] 设置头衔");
            return Text(UnknownError);
        }
        catch (OperationFailedException e)
        {
            Console.WriteLine($"{e.Message} ({e.HResult})");
            return Text(OperationFailed);
        }
    }

    [Help("展示视频信息", "code")]
    [HelpArgs(typeof(string))]
    private static async Task<MessageBuilder> Bv(TextChain text)
    {
        if (text.Content[3..].Bv2Av() is not { } avCode)
            return Text("BV号不对哦");
        // UrlDownload the page
        var html = await $"https://www.bilibili.com/video/{avCode}".UrlDownloadString();
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

    [Help("仓库信息", "organization", "repository")]
    [HelpArgs(typeof(string), typeof(string))]
    private static async Task<MessageBuilder> GitHub(Bot bot, GroupMessageEvent group, TextChain text)
    {
        // UrlDownload the page
        try
        {
            var args = text.Content.Split(' ');
            _ = await bot.SendGroupMessage(group.GroupUin, Text("Fetching repository..."));
            var html = await $"https://github.com/{args[1]}/{args[2]}.git".UrlDownloadString();
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

    [Help("帮我选一个", "items")]
    [HelpArgs(typeof(string[]))]
    private static MessageBuilder Roll(TextChain text)
    {
        var items = text.Content[5..].Trim().Split(' ');
        return Text($"嗯让我想想ww......果然还是\"{items.RandomGet()}\"比较好！");
    }

}