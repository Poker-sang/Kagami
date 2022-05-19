using Kagami.ArgTypes;
using Kagami.Attributes;
using Kagami.Enums;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;

namespace Kagami.Triggers;
public static class Reread
{
    private static readonly Dictionary<uint, (uint Count, string LastText)> RereadDictionary = new();

    /// <summary>
    /// 超过三条连续相同消息复读
    /// "复读一次这条消息", "3连相同文字消息"
    /// </summary>
    /// <param name="group"></param>
    /// <returns></returns>
    [KagamiTrigger(TriggerPriority.AfterCmdlet)]
    public static async ValueTask<bool> RereadMessageAsync(Bot bot, GroupMessageEvent group, Raw raw)
    {
        if (!RereadDictionary.ContainsKey(group.GroupUin))
            RereadDictionary[group.GroupUin] = (1, "");
        var (count, lastText) = RereadDictionary[group.GroupUin];
        try
        {
            if (!string.IsNullOrEmpty(raw.RawString))
                // 如果不是复读
                if (lastText != raw.RawString)
                {
                    lastText = raw.RawString;
                    count = 1;
                }
                // 如果是复读
                // 如果已经出现3次
                else if (count is 2)
                {
                    count = 0;
                    _ = await bot.SendGroupMessage(group.GroupUin, new MessageBuilder(lastText));
                    return true;
                }
                // 如果没有3次且没有复读过
                else if (lastText == raw.RawString && count is not 0)
                    ++count;
        }
        finally
        {
            RereadDictionary[group.GroupUin] = (count, lastText);
        }

        return false;
    }
}
