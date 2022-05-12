using System.Diagnostics;
using Kagami.Utils;
using Konata.Core;
using Konata.Core.Common;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Konata.Core.Message.Model;

namespace Kagami.Services;

/// <summary>
/// 和Kernel相关的业务逻辑代码
/// </summary>
public static class Kernel
{
    private static readonly WeakReference<MessageBuilder?> s_ping = new(null);
    private static readonly WeakReference<MessageBuilder?> s_greeting = new(null);

    /// <summary>
    /// Ping
    /// </summary>
    public static MessageBuilder Ping => s_ping.Get(() => new("Pong!"));

    /// <summary>
    /// 自我介绍
    /// </summary>
    public static MessageBuilder Greeting => s_greeting.Get(() => new("你好!!! 我是Poker Kagami!"));

    /// <summary>
    /// 复读机
    /// </summary>
    /// <param name="message">消息</param>
    /// <returns></returns>
    public static MessageBuilder Repeat(MessageChain message) => new(message[1..]);

    /// <summary>
    /// 获取成员信息
    /// </summary>
    /// <param name="at"></param>
    /// <param name="bot"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static async Task<MessageBuilder> Member(AtChain at!!, Bot bot!!, GroupMessageEvent group!!)
    {
        // Get group info
        var memberInfo = await bot.GetGroupMemberInfo(group.GroupUin, at.AtUin, true);
        if (memberInfo is null)
            return new("没有找到这个人x");

        return new MessageBuilder($"[{memberInfo.NickName}]")
            .TextLine($"群名片：{memberInfo.Name}")
            .TextLine($"加入时间：{memberInfo.JoinTime}")
            .TextLine($"类别：{memberInfo.Role switch
            {
                RoleType.Member => "成员",
                RoleType.Admin => "管理员",
                RoleType.Owner => "群主",
                _ => throw new ArgumentOutOfRangeException("memberInfo.Role", "未知的类别")
            }}")
            .TextLine($"等级：{memberInfo.Level}")
            .TextLine($"头衔：{memberInfo.SpecialTitle}");
    }

    /// <summary>
    /// 状态
    /// </summary>
    /// <returns></returns>
    public static MessageBuilder Status()
    => new MessageBuilder("[Poker Kagami] 内核信息")
        // Core descriptions
        .TextLine($"[分支:{BuildStamp.Branch}]")
        .TextLine($"[提交:{BuildStamp.CommitHash[..12]}]")
        .TextLine($"[版本:{BuildStamp.Version}]")
        .TextLine($"[{BuildStamp.BuildTime}]")
        .TextLine("Konata Project (C) 2022")
        .TextLine()
        // System status
        // .TextLine($"处理了 {_messageCounter} 条消息")
        .TextLine($"GC内存 {GC.GetTotalAllocatedBytes().Bytes2MiB(2)} MiB ")
        .Text($"({Math.Round((double)GC.GetTotalAllocatedBytes() / GC.GetTotalMemory(false) * 100, 2)}%)")
        .TextLine($"总内存 {Process.GetCurrentProcess().WorkingSet64.Bytes2MiB(2)} MiB");



    public static async Task<MessageBuilder> RollAsync(params string[] items)
    {
        return new (
            items.Length < 2
                ? "没有选项让我怎么选，笨！"
                : string.Format(await StringResources.RollMessage.RandomGetAsync(), await items.RandomGetAsync()));
    }
}