using Kagami.Attributes;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using System;

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
        {
            _lastMessage = group.Message;
            return;
        }

        try
        {
            if (!RereadDictionary.ContainsKey(group.GroupUin))
                RereadDictionary[group.GroupUin] = (1, "");
            if (await GetReply(bot, group) is { } reply)
            {
                RereadDictionary[group.GroupUin] = (RereadDictionary[group.GroupUin].Count, "");
                _ = await bot.SendGroupMessage(group.GroupUin, reply);
            }
            else if (Reread(group) is { } reread)
                _ = await bot.SendGroupMessage(group.GroupUin, reread);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.StackTrace);

            // Send error print
            _ = await bot.SendGroupMessage(group.GroupUin, Text($"{e.Message}"));
        }
    }

    private const string EnvPath = @"C:\Users\poker\Desktop\Bot\";
    public const string HelpPath = EnvPath + @"help\";

    private const string HelpImage = HelpPath + "help.png";
}