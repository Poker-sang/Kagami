using Kagami.Core;
using Konata.Core.Common;
using Konata.Core.Message;
using Konata.Core.Message.Model;
using System.Diagnostics;

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
    public static MessageBuilder Repeat(MessageChain chain)
        => new MessageBuilder(((TextChain)chain[0]).Content[nameof(Repeat).Length..].Trim()).Add(chain[1..]);

    /// <summary>
    /// 获取成员信息
    /// </summary>
    /// <param name="at"></param>
    /// <param name="bot"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static MessageBuilder Member(BotMember memberInfo)
        => new MessageBuilder($"[{memberInfo.NickName}]")
            .TextLine($"群名片：{memberInfo.Name}")
            .TextLine($"加入时间：{(memberInfo.JoinTime is 0
                ? "未知" : DateTimeOffset.FromUnixTimeSeconds(memberInfo.JoinTime).LocalDateTime)}")
            .TextLine($"类别：{memberInfo.Role switch
            {
                RoleType.Member => "成员",
                RoleType.Admin => "管理员",
                RoleType.Owner => "群主",
                _ => throw new ArgumentOutOfRangeException("memberInfo.Role", "未知的类别")
            }}")
            .TextLine($"等级：{memberInfo.Level}")
            .TextLine($"头衔：{memberInfo.SpecialTitle}");

    /// <summary>
    /// 状态
    /// </summary>
    /// <returns></returns>
    public static MessageBuilder Status()
    => new MessageBuilder("[Konata.Core] 内核信息")
        // Core descriptions
        .TextLine($"[分支:{KonataBuildStamp.Branch}]")
        .TextLine($"[提交:{KonataBuildStamp.CommitHash[..12]}]")
        .TextLine($"[版本:{KonataBuildStamp.Version}]")
        .TextLine($"[{KonataBuildStamp.BuildTime}]")
        .TextLine("Konata Project © 2022")
        .TextLine()
        .TextLine("[Poker Kagami] 构建信息")
        .TextLine($"[分支:{KagamiBuildStamp.Branch}]")
        .TextLine($"[提交:{KagamiBuildStamp.Revision}]")
        .TextLine($"[版本:{KagamiBuildStamp.Version}]")
        .TextLine($"[{KagamiBuildStamp.BuildTime}]")
        .TextLine("Poker Kagami Project © 2022 frg2089 & Poker, All Rights Reserved.")
        .TextLine()
        // System status
        .TextLine($"处理了 {BotResponse.MessageCounter.Values.Aggregate(0, (a, b) => a + b)} 条消息")
        .TextLine($"GC内存 {GC.GetTotalAllocatedBytes().Bytes2MiB(2)} MiB ")
        .Text($"({Math.Round((double)GC.GetTotalAllocatedBytes() / GC.GetTotalMemory(false) * 100, 2)}%)")
        .TextLine($"总内存 {Process.GetCurrentProcess().WorkingSet64.Bytes2MiB(2)} MiB");

    public static MessageBuilder Roll(string[] items)
        => new(items.Length < 2
            ? "没有选项让我怎么选，笨！"
            : string.Format(StringResources.RollMessage.RandomGet(), items.RandomGet()));


    public static double Bytes2MiB(this long bytes, int round) => Math.Round(bytes / 1048576.0, round);
}
