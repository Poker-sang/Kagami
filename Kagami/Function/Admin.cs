using System;
using System.Threading.Tasks;
using Kagami.ArgTypes;
using Kagami.Attributes;
using Konata.Core;
using Konata.Core.Common;
using Konata.Core.Events.Model;
using Konata.Core.Exceptions.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Konata.Core.Message.Model;

namespace Kagami.Function;

public static partial class Commands
{

    [Permission(RoleType.Admin)]
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

    [Permission(RoleType.Owner)]
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
            if (await bot.GroupSetSpecialTitle(group.GroupUin, atChain.AtUin, textChains[1].Content.Trim(), uint.MaxValue))
                return Text($"为 [{atChain.AtUin}] 设置头衔");
            return Text(UnknownError);
        }
        catch (OperationFailedException e)
        {
            Console.WriteLine($"{e.Message} ({e.HResult})");
            return Text(OperationFailed);
        }
    }
}