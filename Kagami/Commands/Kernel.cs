using Kagami.ArgTypes;
using Kagami.UsedTypes;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Konata.Core.Message.Model;
using System.ComponentModel;

namespace Kagami.Commands;

public static class Kernel
{
    //private static readonly WeakReference<MessageBuilder?> sPing = new(null);

    [Cmdlet(nameof(Ping)), Description("看看我是否还在线")]
    public static MessageBuilder Ping() => new("Pong!");
    // sPing.Get(() => new MessageBuilder().Record(@"C:\WorkSpace\Kagami\ping.amr"));

    [Cmdlet(nameof(Greeting), "自我介绍"), Description("自我介绍")]
    public static MessageBuilder Greeting() => new("你好!!! 我是Poker Kagami!");

    [Cmdlet(nameof(Status), "状态"), Description("内核信息")]
    public static MessageBuilder Status() => new(Services.Kernel.Status());

    [Obsolete("容易被封禁")]
    [Cmdlet(nameof(Repeat), "复读"), Description("复读一条消息")]
    public static MessageBuilder Repeat(GroupMessageEvent group)
        => new MessageBuilder(((TextChain)group.Chain[0]).Content[nameof(Repeat).Length..].Trim()).Add(group.Chain[1..]);

    [Cmdlet(nameof(Roll), "随机"), Description("帮我选一个")]
    public static MessageBuilder Roll([Description("一些选项")] string[] items)
        => new(Services.Kernel.Roll(items[1..]));

    [Cmdlet(nameof(Member), "成员"), Description("获取成员信息")]
    public static async ValueTask<MessageBuilder> Member(Bot bot, GroupMessageEvent group,
        [Description("成员")] At at)
        => new(await bot.GetGroupMemberInfo(group.GroupUin, at.Uin, true) is { } memberInfo
            ? Services.Kernel.Member(memberInfo)
            : "没有找到这个人x");
}
