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


namespace Kagami.Function;

public static partial class Commands
{
    [Help("Get Status")]
    private static MessageBuilder Status()
        => Text(
            // Core descriptions
            $"[Poker Kagami]\n" +
            $"[branch:{BuildStamp.Branch}]\n" +
            $"[commit:{BuildStamp.CommitHash[..12]}]\n" +
            $"[version:{BuildStamp.Version}]\n" +
            ($"[{BuildStamp.BuildTime}]\n\n" +

            // System status
            $"Processed {_messageCounter} message(s)\n" +
            $"GC Memory {GC.GetTotalAllocatedBytes().Bytes2MiB(2)} MiB " +
                  $"({Math.Round((double)GC.GetTotalAllocatedBytes() / GC.GetTotalMemory(false) * 100, 2)}%)\n" +
            $"Total Memory {Process.GetCurrentProcess().WorkingSet64.Bytes2MiB(2)} MiB\n\n" +

            // Copyrights
            "Konata Project (C) 2022");

    [Help("Greeting")]
    private static MessageBuilder Greeting() => Text("Hello, I'm Poker Kagami");

    [Help("-message Repeat a message")]
    private static MessageBuilder Repeat(TextChain text, MessageChain message) => Text(text.Content[4..].Trim()).Add(message[1..]);

    [Help("-at Get MemberInfo")]
    private static async Task<MessageBuilder> Member(Bot bot, GroupMessageEvent group)
    {
        // Get at
        var at = group.Chain.GetChain<AtChain>();
        if (at is null)
            return Text("Argument error");

        // Get group info
        var memberInfo = await bot.GetGroupMemberInfo(group.GroupUin, at.AtUin, true);
        if (memberInfo is null)
            return Text("No such member");

        return Text(
            "[Member Info]\n" +
            $"Name: {memberInfo.Name}\n" +
            $"Join: {memberInfo.JoinTime}\n" +
            $"Role: {memberInfo.Role}\n" +
            $"Level: {memberInfo.Level}\n" +
            $"SpecTitle: {memberInfo.SpecialTitle}\n" +
            $"Nickname: {memberInfo.NickName}");
    }

    [Help("-at [-minute] Mute a member")]
    private static async Task<MessageBuilder> Mute(Bot bot, GroupMessageEvent group)
    {
        // Get at
        var atChain = group.Chain.GetChain<AtChain>();
        if (atChain is null)
            return Text("Argument error");

        var time = 10U;
        var textChains = group.Chain.FindChain<TextChain>();
        // Parse time
        if (textChains.Count is 2 &&
            uint.TryParse(textChains[1].Content, out var t))
            time = t;

        try
        {
            if (await bot.GroupMuteMember(group.GroupUin, atChain.AtUin, time * 60))
                return Text($"Mute member [{atChain.AtUin}] for {time} minutes.");
            return Text("Unknown error.");
        }
        catch (OperationFailedException e)
        {
            return Text($"{e.Message} ({e.HResult})");
        }
    }

    [Help("-at -title Set Title for member")]
    private static async Task<MessageBuilder> Title(Bot bot, GroupMessageEvent group)
    {
        // Get at
        var atChain = group.Chain.GetChain<AtChain>();
        if (atChain is null)
            return Text("Argument error");

        var textChains = group.Chain.FindChain<TextChain>();
        // Check argument
        if (textChains.Count is not 2)
            return Text("Argument error");

        try
        {
            if (await bot.GroupSetSpecialTitle(group.GroupUin, atChain.AtUin, textChains[1].Content, uint.MaxValue))
                return Text($"Set special title for member [{atChain.AtUin}].");
            return Text("Unknown error.");
        }
        catch (OperationFailedException e)
        {
            return Text($"{e.Message} ({e.HResult})");
        }
    }

    [Help("-code Parse bv to av")]
    private static async Task<MessageBuilder> Bv(TextChain text)
    {
        var avCode = text.Content[4..].Bv2Av();
        if (avCode is "")
            return Text("Invalid BV code");
        // UrlDownload the page
        var html = await $"https://www.bilibili.com/video/{avCode}".UrlDownloadString();
        // Get meta data
        var metaData = html.GetMetaData("itemprop");
        var titleMeta = metaData["description"];
        var imageMeta = metaData["image"];
        var keywordMeta = metaData["keywords"];

        // UrlDownload the image
        var image = await imageMeta.UrlDownloadBytes();

        // Build message
        return Text($"{titleMeta}\n")
            .Text($"https://www.bilibili.com/video/{avCode}\n\n")
            .Image(image)
            .Text("\n#" + string.Join(" #", keywordMeta.Split(",")[1..^4]));
    }

    [Help("-organization -repository Github repo")]
    private static async Task<MessageBuilder> Github(Bot bot, GroupMessageEvent group, TextChain text)
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
            return Text("Not a repository link.");
        }
    }
}