using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Kagami.Attributes;
using Kagami.Utils;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Exceptions.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Konata.Core.Message.Model;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedParameter.Local

namespace Kagami.Function;

[GenerateHelp("Poker Kagami Help", DefaultPrefix = "\\")]
public static partial class Command
{
    private static uint _messageCounter;

    /// <summary>
    /// On group message
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="group"></param>
    internal static async void OnGroupMessage(Bot bot, GroupMessageEvent group)
    {
        // Increase
        ++_messageCounter;

        if (group.MemberUin == bot.Uin) 
            return;


        try
        {
            if (await GetReply(bot, group) is { } reply)
                await bot.SendGroupMessage(group.GroupUin, reply);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);

            // Send error print
            await bot.SendGroupMessage(group.GroupUin,
                Text($"{e.Message}\n{e.StackTrace}"));
        }
    }

    [Help("Get Status")]
    public static MessageBuilder Status()
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
    public static MessageBuilder Greeting()
        => Text("Hello, I'm Poker Kagami");

    [Help("Echo a message(Safer than Eval)")]
    public static MessageBuilder Echo(TextChain text, MessageChain chain)
        => new MessageBuilder(text.Content[5..].Trim()).Add(chain[1..]);

    [Help("Eval a message")]
    public static MessageBuilder Eval(MessageChain chain)
        => MessageBuilder.Eval(chain.ToString()[5..].TrimStart());

    [Help("Get MemberInfo")]
    public static async Task<MessageBuilder> MemberInfo(Bot bot, GroupMessageEvent group)
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
    public static async Task<MessageBuilder> Mute(Bot bot, GroupMessageEvent group)
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
            {
                time = t;
            }
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
    public static async Task<MessageBuilder> SetTitle(Bot bot, GroupMessageEvent group)
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
    public static async Task<MessageBuilder> Bv(TextChain chain)
    {
        var avCode = chain.Content[4..].Bv2Av();
        if (avCode is "") 
            return Text("Invalid BV code");
        // UrlDownload the page
        var bytes = await $"https://www.bilibili.com/video/{avCode}".UrlDownload();
        var html = Encoding.UTF8.GetString(bytes);
        // Get meta data
        var metaData = html.GetMetaData("itemprop");
        var titleMeta = metaData["description"];
        var imageMeta = metaData["image"];
        var keyWdMeta = metaData["keywords"];

        // UrlDownload the image
        var image = await imageMeta.UrlDownload();

        // Build message
        var result = new MessageBuilder();
        result.Text($"{titleMeta}\n");
        result.Text($"https://www.bilibili.com/video/{avCode}\n\n");
        result.Image(image);
        result.Text("\n#" + string.Join(" #", keyWdMeta.Split(",")[1..^4]));
        return result;
    }

    [Help("Github repo parser", Name = "https://github.com/")]
    public static async Task<MessageBuilder> GithubParser(TextChain chain)
    {
        // UrlDownload the page
        try
        {
            var bytes = await $"{chain.Content.TrimEnd('/')}.git".UrlDownload();
            var html = Encoding.UTF8.GetString(bytes);
            // Get meta data
            var metaData = html.GetMetaData("property");
            var imageMeta = metaData["og:image"];

            // Build message
            var image = await imageMeta.UrlDownload();
            return new MessageBuilder().Image(image);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Not a repository link. \n{e.Message}");
            return null;
        }
    }

    [Help("Repeat a message")]
    public static MessageBuilder Repeat(MessageChain message) => new(message);

    private static MessageBuilder Text(string text) => new MessageBuilder().Text(text);
}