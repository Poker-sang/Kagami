using Kagami.ArgTypes;
using Kagami.Attributes;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using System.ComponentModel;

namespace Kagami.Commands;

public static class Kernel
{
    [Cmdlet(nameof(Ping)), Description("看看我是否还在线")]
    public static MessageBuilder Ping() => Services.Kernel.Ping;

    [Cmdlet(nameof(Greeting)), Description("自我介绍")]
    public static MessageBuilder Greeting() => Services.Kernel.Greeting;

    [Cmdlet(nameof(Status)), Description("内核信息")]
    public static MessageBuilder Status() => Services.Kernel.Status();

    [Cmdlet(nameof(Repeat)), Description("复读一条消息")]
    public static MessageBuilder Repeat(GroupMessageEvent group) => Services.Kernel.Repeat(group.Chain);

    [Cmdlet(nameof(Roll)), Description("帮我选一个")]
    public static MessageBuilder Roll([Description("一些选项")] string[] items)
        => Services.Kernel.Roll(items[1..]);

    [Cmdlet(nameof(Member)), Description("获取成员信息")]
    public static async ValueTask<MessageBuilder> Member(Bot bot, GroupMessageEvent group,
        [Description("成员")] At at)
        => await bot.GetGroupMemberInfo(group.GroupUin, at.Uin, true) is { } memberInfo
            ? Services.Kernel.Member(memberInfo)
            : new("没有找到这个人x");
}
