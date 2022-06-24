using Kagami.ArgTypes;
using Kagami.Attributes;
using Kagami.Core;
using Kagami.UsedTypes;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Konata.Core.Message.Model;

namespace Kagami.Triggers;

/// <summary>
/// 复读
/// </summary>
public static class Reread
{
    /// <summary>
    /// 键是群号Uin<br/>
    /// 值1是上次处理的消息的序号（防止漏过消息）<br/>
    /// 值2是消息连续出现次数<br/>
    /// 值3是消息内容<br/>
    /// </summary>
    /// <remarks>
    /// 例如：<br/>
    /// 1. aaa<br/>
    /// 2. bbb<br/>
    /// 3. bbb<br/>
    /// 此时值为（3, 2, "bbb"）<br/><br/>
    /// - 若新消息为 4. bbb<br/>
    /// 新消息相同且没有漏消息，之前已出现2次，复读<br/>
    /// 值设为(0, 0, "bbb")，0表示该消息不再自增（不再复读）<br/><br/>
    /// - 若新消息为 4. ccc<br/>
    /// 新消息不同，新值设为(4, 1, "ccc")<br/><br/>
    /// - 若新消息为 5. bbb<br/>
    /// 新消息相同，但不连续（漏消息），新值设为(5, 1, "bbb")<br/>
    /// </remarks>
    private static readonly Dictionary<uint, (int LastCount, int MessageCount, string LastText)> rereadDictionary = new();

    private static Dictionary<uint, int> MessageCounter => BotResponse.MessageCounter;

    /// <summary>
    /// 超过三条连续相同消息复读
    /// "复读一次这条消息", "3连相同文字消息"
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    [Trigger(TriggerPriority.AfterCmdlet)]
    public static async ValueTask<bool> RereadMessageAsync(Bot bot, GroupMessageEvent group)
    {
        var text = "";
        if (group.Chain is { Count: 1 } && group.Chain[0] is TextChain textChain)
            text = textChain.Content;
        var messageCount = 1;
        try
        {
            if (!rereadDictionary.ContainsKey(group.GroupUin))
                return false;
            (var lastCount, messageCount, var lastText) = rereadDictionary[group.GroupUin];
            // 如果有漏消息
            if (lastCount + 1 != MessageCounter[group.GroupUin])
                messageCount = 1;
            // 如果没有漏消息
            // 如果不是重复的
            else if (text is "" || lastText != text)
                messageCount = 1;
            // 如果是重复的
            // 如果已经出现3次
            else if (messageCount is 2)
            {
                messageCount = 0;
                _ = await bot.SendGroupMessage(group.GroupUin, new MessageBuilder(lastText));
                return true;
            }
            // 如果没有3次且没有复读过
            else if (messageCount is not 0)
                ++messageCount;
        }
        finally
        {
            rereadDictionary[group.GroupUin] = (MessageCounter[group.GroupUin], messageCount, text);
        }

        return false;
    }
}
