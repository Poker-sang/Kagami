using System.ComponentModel;
using Kagami.ArgTypes;
using Kagami.Attributes;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;

namespace Kagami.Commands;

public static class Kernel
{
    [KagamiCmdlet(nameof(Ping)), Description("看看我是否还在线")]
    public static MessageBuilder Ping() => Services.Kernel.Ping;

    [KagamiCmdlet(nameof(Greeting)), Description("自我介绍")]
    public static MessageBuilder Greeting() => Services.Kernel.Greeting;

    [KagamiCmdlet(nameof(Status)), Description("内核信息")]
    public static MessageBuilder Status() => Services.Kernel.Status();

    [KagamiCmdlet(nameof(Repeat)), Description("复读一条消息")]
    public static MessageBuilder Repeat(GroupMessageEvent group) => Services.Kernel.Repeat(group.Chain);

    [KagamiCmdlet(nameof(Roll)), Description("帮我选一个")]
    public static async ValueTask<MessageBuilder> Roll([Description("一些选项")] string[] items)
        => await Services.Kernel.RollAsync(items);

    [KagamiCmdlet(nameof(Member)), Description("获取成员信息")]
    public static async ValueTask<MessageBuilder> Member(Bot bot, GroupMessageEvent group,
        [Description("成员")] At at)
        => await bot.GetGroupMemberInfo(group.GroupUin, at.Uin, true) is { } memberInfo
            ? Services.Kernel.Member(memberInfo)
            : (new("没有找到这个人x"));
}
