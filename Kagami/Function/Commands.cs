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
using System.Text;
using System.Threading.Tasks;


namespace Kagami.Function;

public static partial class Commands
{
    [Help("Get Status")]
    private static MessageBuilder Status()
        => new MessageBuilder()
            // Core descriptions
            .Text($"[Poker Kagami]\n")
            .Text($"[branch:{BuildStamp.Branch}]\n")
            .Text($"[commit:{BuildStamp.CommitHash[..12]}]\n")
            .Text($"[version:{BuildStamp.Version}]\n")
            .Text($"[{BuildStamp.BuildTime}]\n\n")

            // System status
            .Text($"Processed {_messageCounter} message(s)\n")
            .Text($"GC Memory {GC.GetTotalAllocatedBytes().Bytes2MiB(2)} MiB " +
                  $"({Math.Round((double)GC.GetTotalAllocatedBytes() / GC.GetTotalMemory(false) * 100, 2)}%)\n")
            .Text($"Total Memory {Process.GetCurrentProcess().WorkingSet64.Bytes2MiB(2)} MiB\n\n")

            // Copyrights
            .Text("Konata Project (C) 2022");

    [Help("Greeting")]
    private static MessageBuilder Greeting()
        => Text("Hello, I'm Poker Kagami");

    [Help("Echo a message(Safer than Eval)")]
    private static MessageBuilder Echo(TextChain text, MessageChain chain)
        => new MessageBuilder(text.Content[4..].Trim()).Add(chain[1..]);

    [Help("Eval a message")]
    private static MessageBuilder Eval(MessageChain chain)
        => MessageBuilder.Eval(chain.ToString()[4..].TrimStart());

    [Help("Get MemberInfo")]
    private static async Task<MessageBuilder> MemberInfo(Bot bot, GroupMessageEvent group)
    {
        // Get at
        var at = group.Chain.GetChain<AtChain>();
        if (at is null)
            return Text("Argument error");

        // Get group info
        var memberInfo = await bot.GetGroupMemberInfo(group.GroupUin, at.AtUin, true);
        if (memberInfo is null)
            return Text("No such member");

        return new MessageBuilder("[Member Info]\n")
            .Text($"Name: {memberInfo.Name}\n")
            .Text($"Join: {memberInfo.JoinTime}\n")
            .Text($"Role: {memberInfo.Role}\n")
            .Text($"Level: {memberInfo.Level}\n")
            .Text($"SpecTitle: {memberInfo.SpecialTitle}\n")
            .Text($"Nickname: {memberInfo.NickName}");
    }

    [Help("Mute a member")]
    private static async Task<MessageBuilder> Mute(Bot bot, GroupMessageEvent group)
    {
        // Get at
        var atChain = group.Chain.GetChain<AtChain>();
        if (atChain is null)
            return Text("Argument error");

        var time = 60U;
        var textChains = group.Chain
            .FindChain<TextChain>();
        {
            // Parse time
            if (textChains.Count is 2 &&
                uint.TryParse(textChains[1].Content, out var t))
                time = t;
        }

        try
        {
            if (await bot.GroupMuteMember(group.GroupUin, atChain.AtUin, time))
                return Text($"Mute member [{atChain.AtUin}] for {time} sec.");
            return Text("Unknown error.");
        }
        catch (OperationFailedException e)
        {
            return Text($"{e.Message} ({e.HResult})");
        }
    }

    [Help("Set Title for member")]
    private static async Task<MessageBuilder> SetTitle(Bot bot, GroupMessageEvent group)
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

    [Help("Parse bv to av")]
    private static async Task<MessageBuilder> Bv(TextChain chain)
    {
        var avCode = chain.Content[4..].Bv2Av();
        if (avCode is "")
            return Text("Invalid BV code");
        // UrlDownload the page
        var html = await $"https://www.bilibili.com/video/{avCode}".UrlDownloadString();
        // Get meta data
        var metaData = html.GetMetaData("itemprop");
        var titleMeta = metaData["description"];
        var imageMeta = metaData["image"];
        var keyWdMeta = metaData["keywords"];

        // UrlDownload the image
        var image = await imageMeta.UrlDownloadBytes();

        // Build message
        var result = new MessageBuilder();
        result.Text($"{titleMeta}\n");
        result.Text($"https://www.bilibili.com/video/{avCode}\n\n");
        result.Image(image);
        result.Text("\n#" + string.Join(" #", keyWdMeta.Split(",")[1..^4]));
        return result;
    }

    [Help("Github repo parser", Name = "https://github.com/")]
    private static async Task<MessageBuilder> GithubParser(Bot bot, GroupMessageEvent group, TextChain chain)
    {
        // UrlDownload the page
        try
        {
            _ = await bot.SendGroupMessage(group.GroupUin, Text("Fetching repository..."));
            var html = await $"{chain.Content.TrimEnd('/')}.git".UrlDownloadString();
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

    [Help("Repeat a message")]
    private static MessageBuilder Repeat(MessageChain message) => new(message);
}