using System.Reflection;
using Kagami.Attributes;
using Kagami.Enums;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;

namespace Kagami.Services;
internal static class Recall
{
    private static readonly Dictionary<string, PropertyInfo> s_props = typeof(MessageStruct)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                ?.ToDictionary(i => i.Name)
                ?? throw new InvalidOperationException($"不能成功反射类型{typeof(MessageStruct).FullName}的属性");

    [KagamiTrigger(TriggerPriority.BeforeCmdlet)]
    public static async ValueTask<bool> RecallBotMessageAsync(Bot bot, GroupMessageEvent group, ArgTypes.Reply reply, string content)
    {
        if (!content.Contains("recall"))
            return false;
        if (bot.Uin == reply.Uin)
        {
            await RecallAsync(bot, group.GroupUin, reply);
            return true;
        }
        return false;
    }

    private static async Task RecallAsync(Bot bot, uint groupid, ArgTypes.Reply reply)
    {
        MessageStruct messageStruct = new(0, "", DateTime.Now);
        s_props[nameof(MessageStruct.Receiver)]
            .SetValue(messageStruct, (groupid, ""));
        s_props[nameof(MessageStruct.Sequence)]
            .SetValue(messageStruct, reply.Sequence);

        try
        {
            if (!await bot.RecallMessage(messageStruct))
                _ = await bot.SendGroupMessage(groupid, new MessageBuilder("撤回失败"));
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            _ = await bot.SendGroupMessage(groupid, new MessageBuilder("呜呜无法撤回了，可能因为超过两分钟或已撤回x"));
        }
    }
}
