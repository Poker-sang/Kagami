﻿using System;
using Kagami.Attributes;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;

namespace Kagami.Function;

[GenerateHelp("Poker Kagami Help")]
public static partial class Commands
{
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
                _ = await bot.SendGroupMessage(group.GroupUin, reply);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);

            // Send error print
            _ = await bot.SendGroupMessage(group.GroupUin,
                Text($"{e.Message}\n{e.StackTrace}"));
        }
    }
}