using Kagami.Core;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;

namespace Kagami.Utilities;

public static class Poke
{
    /// <summary>
    /// On group poke
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="group"></param>
    internal static void OnGroupPoke(Bot bot, GroupPokeEvent group)
    {
        if (group.MemberUin != bot.Uin)
            return;

        // Convert it to ping
        if (Random.Shared.Next(10) is 0)
            _ = bot.SendGroupMessage(
                group.GroupUin,
                new Konata.Core.Message.MessageBuilder(StringResources.PokeMessage.RandomGet()));
    }
}
