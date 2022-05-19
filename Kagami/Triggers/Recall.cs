using Kagami.Attributes;
using Kagami.Enums;
using Konata.Core;
using Konata.Core.Events.Model;
using static Kagami.Services.Recall;

namespace Kagami.Triggers;

public static class Recall
{
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
}
