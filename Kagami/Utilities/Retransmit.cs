using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using System.Text.Json;

namespace Kagami.Utilities;

public static class Retransmit
{
    private record RetransmitData(uint FriendUin, uint GroupUin);

    public static uint FriendUin { get; set; }

    public static uint GroupUin { get; set; }

    public static void OnFriendMessage(Bot bot, FriendMessageEvent e)
    {
        if (FriendUin is not 0 && GroupUin is not 0)
            if (e.FriendUin == FriendUin)
                _ = bot.SendGroupMessage(GroupUin, new MessageBuilder(e.Chain));
    }

    public static void Save()
    {
        var retransmit = JsonSerializer.Serialize(new RetransmitData(FriendUin, GroupUin),
            new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("retransmit.json", retransmit);
    }

    public static void TryLoad()
    {
        if (File.Exists("retransmit.json") && JsonSerializer.Deserialize
                <RetransmitData>(File.ReadAllText("retransmit.json")) is { } retransmit)
        {
            FriendUin = retransmit.FriendUin;
            GroupUin = retransmit.GroupUin;
        }
    }
}
