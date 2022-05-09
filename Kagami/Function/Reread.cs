using Konata.Core.Events.Model;
using Konata.Core.Message;
using Konata.Core.Message.Model;
using System.Collections.Generic;
using Kagami.Attributes;

namespace Kagami.Function;

public static partial class Commands
{
    private static readonly Dictionary<uint, (uint Count, string LastText)> RereadDictionary = new();

    /// <summary>
    /// 超过三条连续相同消息复读
    /// </summary>
    /// <param name="group"></param>
    /// <returns></returns>
    [Trigger("复读一次这条消息", "3连相同文字消息")]
    private static MessageBuilder? Reread(GroupMessageEvent group)
    {
        var (count, lastText) = RereadDictionary[group.GroupUin];
        try
        {
            if (group.Chain.Count is 1 && group.Chain[0] is TextChain text)
                // 如果不是复读
                if (lastText != text.Content)
                {
                    lastText = text.Content;
                    count = 1;
                }
                // 如果是复读
                // 如果已经出现3次
                else if (count is 2)
                {
                    count = 0;
                    return Text(lastText);
                }
                // 如果没有3次且没有复读过
                else if (lastText == text.Content && count is not 0)
                    ++count;
        }
        finally
        {
            RereadDictionary[group.GroupUin] = (count, lastText);
        }

        return null;
    }
}