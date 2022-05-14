using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;

namespace Kagami.Function;

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
        if (new Random().Next(10) is 0)
            _ = bot.SendGroupMessage(group.GroupUin, Commands.Text(PokeMessage.RandomGet()));
    }

    private static readonly string[] PokeMessage = { "你再戳", "rua", "不许戳", "戳♥死♥我", "呜呜", "别戳了别戳了", "啊啊啊", "。", "？", "！", "喵", "呜", "您？" };
}