using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Konata.Core.Message.Model;

namespace Kagami.Services;
internal static class Recall
{
    private static readonly Dictionary<string, PropertyInfo> s_props = typeof(MessageStruct)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                ?.ToDictionary(i => i.Name)
                ?? throw new InvalidOperationException($"不能成功反射类型{typeof(MessageStruct).FullName}的属性");


    //[Trigger("撤回我被回复的消息", "回复我的某条信息")]
    public static async Task<MessageBuilder?> RecallAsync(Bot bot, uint groupid, ArgTypes.Reply reply)
    {
        MessageStruct messageStruct = new(0, "", DateTime.Now);
        s_props[nameof(MessageStruct.Receiver)]
            .SetValue(messageStruct, (groupid, ""));
        s_props[nameof(MessageStruct.Sequence)]
            .SetValue(messageStruct, reply.Sequence);

        try
        {
            if (await bot.RecallMessage(messageStruct))
                return null;
            else
                return new("撤回失败");
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            if (bot.Uin == reply.Uin)
                return new("呜呜超过两分钟无法撤回了，麻烦联系管理员x");
            else
                return new("只有管理员才有权力撤回别人的消息");
        }
    }
}
