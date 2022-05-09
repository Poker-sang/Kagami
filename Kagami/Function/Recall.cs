using Kagami.Utils;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Konata.Core.Message.Model;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Kagami.Attributes;

namespace Kagami.Function;

public static partial class Commands
{
    [Trigger("撤回我被回复的消息", "回复我的某条信息")]
    private static async Task<bool> Recall(Bot bot, GroupMessageEvent group)
    {
        if (group.Chain.FetchChain<ReplyChain>() is { } replyChain)
        {
            if (group.Chain.FetchChains<TextChain>().All(t => !t.Content.ToLower().Contains("recall")))
                return false;

            var replyChainProperties = typeof(ReplyChain).GetProperties(BindingFlags.NonPublic | BindingFlags.Instance);
            uint uin = 0;
            uint sequence = 0;
            foreach (var propertyInfo in replyChainProperties)
                switch (propertyInfo.Name)
                {
                    case "Uin":
                        uin = (uint)propertyInfo.GetValue(replyChain)!;
                        break;
                    case "Sequence":
                        sequence = (uint)propertyInfo.GetValue(replyChain)!;
                        break;
                }
            if (uin == bot.Uin)
            {
                var messageStruct = new MessageStruct(0, "", DateTime.Now);
                var messageStructProperties = typeof(MessageStruct).GetProperties(BindingFlags.Public | BindingFlags.Instance) ?? throw new ArgumentNullException("typeof(MessageStruct).GetProperties(BindingFlags.Public | BindingFlags.Instance)");
                foreach (var propertyInfo in messageStructProperties)
                    switch (propertyInfo.Name)
                    {
                        case "Receiver":
                            propertyInfo.SetValue(messageStruct, (group.GroupUin, ""));
                            break;
                        case "Sequence":
                            propertyInfo.SetValue(messageStruct, sequence);
                            break;
                    }

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