using Kagami.ArgTypes;
using Kagami.Attributes;
using Kagami.Enums;
using Konata.Core;
using Konata.Core.Events.Model;
using static Kagami.Services.Recall;

namespace Kagami.Triggers;

/// <summary>
/// 撤回
/// </summary>
public static class Recall
{
    /// <summary>
    /// 撤回机器人消息
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="group"></param>
    /// <param name="reply"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    [Trigger(TriggerPriority.BeforeCmdlet)]
    public static async ValueTask<bool> RecallBotMessageAsync(Bot bot, GroupMessageEvent group, Reply reply, string content)
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
}
