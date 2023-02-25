using Kagami.Core;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Konata.Core.Message.Model;
using System.Text.Json;

namespace Kagami.Utilities;

public static class Retransmit
{
    private record RetransmitData(uint FriendUin, uint GroupUin);

    public static uint FriendUin { get; set; }

    public static uint GroupUin { get; set; }

    public static async void OnFriendMessage(Bot bot, FriendMessageEvent e)
    {
        if (FriendUin is not 0 && GroupUin is not 0)
            if (e.FriendUin == FriendUin)
                if (e.Chain is [TextChain textChain] && textChain.Content.StartsWith('/'))
                    try
                    {
                        Console.WriteLine(textChain.Content);
                        await Program.CommandLineInterface(textChain.Content);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        await Program.Exit();
                    }
                else
                    _ = bot.SendGroupMessage(GroupUin, new MessageBuilder(e.Chain));
    }

    private const string RetransmitPath = Paths.UtilitiesPath + "retransmit.json";

    public static void Save()
    {
        var retransmit = JsonSerializer.Serialize(new RetransmitData(FriendUin, GroupUin),
            new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(RetransmitPath, retransmit);
    }

    public static void TryLoad()
    {
        if (File.Exists(RetransmitPath) && JsonSerializer.Deserialize
                <RetransmitData>(File.ReadAllText(RetransmitPath)) is { } retransmit)
        {
            FriendUin = retransmit.FriendUin;
            GroupUin = retransmit.GroupUin;
        }
    }
}
