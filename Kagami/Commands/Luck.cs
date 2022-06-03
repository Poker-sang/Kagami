using Kagami.Attributes;
using Konata.Core.Events.Model;
using Konata.Core.Message;
using System.ComponentModel;

namespace Kagami.Commands;

public static class Luck
{

    [Cmdlet(nameof(Luck)), Description("今日运势")]
    public static MessageBuilder GetLuck(GroupMessageEvent group)
    {
        var value = Services.Luck.GetValue(group.MemberUin);
        return new MessageBuilder().At(group.MemberUin)
            .TextLine($"今日运势：{value.Draw}")
            .TextLine($"宜：{value.Should[0]}，{value.Should[1]}，{value.Should[2]}")
            .TextLine($"忌：{value.Avoid[0]}，{value.Avoid[1]}，{value.Avoid[2]}");
    }
}
