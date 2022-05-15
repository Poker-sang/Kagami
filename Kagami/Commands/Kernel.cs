using Kagami.ArgTypes;
using Kagami.Attributes;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using System.ComponentModel;

namespace Kagami.Commands;

[KagamiCmdletClass]
public static class Kernel
{
    [KagamiCmdlet(nameof(Ping)), Description("�������Ƿ�����")]
    public static MessageBuilder Ping() => Services.Kernel.Ping;

    [KagamiCmdlet(nameof(Greeting)), Description("���ҽ���")]
    public static MessageBuilder Greeting() => Services.Kernel.Greeting;

    [KagamiCmdlet(nameof(Status)), Description("�ں���Ϣ")]
    public static MessageBuilder Status() => Services.Kernel.Status();

    [KagamiCmdlet(nameof(Repeat)), Description("����һ����Ϣ")]
    public static MessageBuilder Repeat(GroupMessageEvent group) => Services.Kernel.Repeat(group.Chain);

    [KagamiCmdlet(nameof(Roll)), Description("����ѡһ��")]
    public static async Task<MessageBuilder> Roll([Description("һЩѡ��")] string[] items)
        => await Services.Kernel.RollAsync(items);

    [KagamiCmdlet(nameof(Member)), Description("��ȡ��Ա��Ϣ")]
    public static async Task<MessageBuilder> Member(Bot bot, GroupMessageEvent group,
        [Description("��Ա")] At at)
        => await bot.GetGroupMemberInfo(group.GroupUin, at.Uin, true) is { } memberInfo
            ? Services.Kernel.Member(memberInfo)
            : (new("û���ҵ������x"));
}
