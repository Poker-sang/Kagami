using Konata.Core.Events.Model;
using Konata.Core.Message;
using Konata.Core.Message.Model;

namespace Kagami.Function;

public static partial class Commands
{
    private static T WithResetText<T>(this T obj)
    {
        _lastText = "";
        return obj;
    }

    private static string _lastText = "";
    private static uint _count;
    private static MessageBuilder Reread(GroupMessageEvent group)
    {
        var reread = "";
        if (group.Chain.Count is 1 && group.Chain[0] is TextChain text)
            // 如果不是复读
            if (_lastText != text.Content)
            {
                _lastText = text.Content;
                _count = 1;
            }
            // 如果是复读
            // 如果已经出现3次
            else if (_count is 2)
            {
                _count = 0;
                reread = _lastText;
            }
            // 如果没有3次且没有复读过
            else if (_lastText == text.Content && _count is not 0)
                ++_count;

        return Text(reread);
    }
}