using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Kagami.Attributes;
using Kagami.Utils;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Konata.Core.Message.Model;

namespace Kagami.Function;

internal static class ReplyChainExtension
{
    private static PropertyInfo[] Properties { get; } = typeof(ReplyChain).GetProperties(BindingFlags.NonPublic | BindingFlags.Instance);
    private static PropertyInfo Uin { get; } = Properties.First(x => x.Name == nameof(Uin));
    private static PropertyInfo Sequence { get; } = Properties.First(x => x.Name == nameof(Sequence));

    internal static uint GetUin(this ReplyChain chain) => (uint)Uin.GetValue(chain)!;
    internal static uint GetSequence(this ReplyChain chain) => (uint)Sequence.GetValue(chain)!;
}

internal static class MessageStructExtension
{
    private static PropertyInfo[] Properties { get; } = typeof(MessageStruct).GetProperties(BindingFlags.NonPublic | BindingFlags.Instance);
    private static PropertyInfo Receiver { get; } = Properties.First(x => x.Name == nameof(Receiver));
    private static PropertyInfo Sequence { get; } = Properties.First(x => x.Name == nameof(Sequence));

    internal static void SetReceiver(this MessageStruct message, uint uin, string name = "") => Receiver.SetValue(message, (uin, name));
    internal static void SetSequence(this MessageStruct message, uint value) => Sequence.SetValue(message, value);
}

public static partial class Commands
{
    [Trigger("撤回我被回复的消息", "回复我的某条信息")]
    private static async Task<bool> Recall(Bot bot, GroupMessageEvent group)
    {
        if (group.Chain.FetchChain<ReplyChain>() is { } replyChain)
        {
            if (group.Chain.FetchChains<TextChain>().All(t => !t.Content.ToLower().Contains("recall")))
                return false;

            uint uin = replyChain.GetUin();
            uint sequence = replyChain.GetSequence();

            if (uin == bot.Uin)
            {
                var messageStruct = new MessageStruct(0, "", DateTime.Now);
                messageStruct.SetReceiver(uin);
                messageStruct.SetSequence(sequence);
                try
                {
                    _ = await bot.RecallMessage(messageStruct);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    _ = await bot.SendGroupMessage(group.GroupUin, Text("呜呜超过两分钟无法撤回了，麻烦联系管理员x"));
                    return false;
                }
                return true;
            }
        }

        return false;
    }
}