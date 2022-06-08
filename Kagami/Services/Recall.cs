using Konata.Core;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using System.Reflection;

namespace Kagami.Services;
internal static class Recall
{
    private static readonly Dictionary<string, PropertyInfo> sProps = typeof(MessageStruct)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                ?.ToDictionary(i => i.Name)
                ?? throw new InvalidOperationException($"不能成功反射类型{typeof(MessageStruct).FullName}的属性");

    internal static async Task RecallAsync(Bot bot, uint groupId, ArgTypes.Reply reply)
    {
        MessageStruct messageStruct = new(0, "", DateTime.Now);
        sProps[nameof(MessageStruct.Receiver)]
            .SetValue(messageStruct, (groupId, ""));
        sProps[nameof(MessageStruct.Sequence)]
            .SetValue(messageStruct, reply.Sequence);

        try
        {
            if (!await bot.RecallMessage(messageStruct))
                _ = await bot.SendGroupMessage(groupId, new MessageBuilder("撤回失败"));
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            _ = await bot.SendGroupMessage(groupId, new MessageBuilder("呜呜无法撤回了，可能因为超过两分钟或已撤回x"));
        }
    }
}
