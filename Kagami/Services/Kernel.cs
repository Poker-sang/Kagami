using Kagami.Core;
using Konata.Core.Common;
using System.Diagnostics;

namespace Kagami.Services;

/// <summary>
/// 和Kernel相关的业务逻辑代码
/// </summary>
public static class Kernel
{
    /// <summary>
    /// 获取成员信息
    /// </summary>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static string Member(BotMember memberInfo)
        => @$"[{memberInfo.NickName}]
群名片：{memberInfo.Name}
加入时间：{(memberInfo.JoinTime is 0 ? "未知" : DateTimeOffset.FromUnixTimeSeconds(memberInfo.JoinTime).LocalDateTime)}
类别：{memberInfo.Role switch
        {
            RoleType.Member => "成员",
            RoleType.Admin => "管理员",
            RoleType.Owner => "群主",
            _ => throw new ArgumentOutOfRangeException(nameof(memberInfo.Role), "未知的类别")
        }}
等级：{memberInfo.Level}
头衔：{memberInfo.SpecialTitle}";

    /// <summary>
    /// 状态
    /// </summary>
    /// <returns></returns>
    public static string Status()
    => @$"[Konata.Core] 内核信息
[分支:{KonataBuildStamp.Branch}]
[提交:{KonataBuildStamp.CommitHash[..12]}]
[版本:{KonataBuildStamp.Version}]
[{KonataBuildStamp.BuildTime}]
Konata Project © 2022
    
[Poker Kagami] 构建信息
[分支:{KagamiBuildStamp.Branch}]
[提交:{KagamiBuildStamp.Revision}]
[版本:{KagamiBuildStamp.Version}]
[{KagamiBuildStamp.BuildTime}]
Poker Kagami Project © 2022 frg2089 & Poker, All Rights Reserved.

处理了 {BotResponse.MessageCounter.Values.Aggregate(0, (a, b) => a + b)} 条消息
GC内存 {GC.GetTotalAllocatedBytes().Bytes2MiB(2)}MiB ({Math.Round((double)GC.GetTotalAllocatedBytes() / GC.GetTotalMemory(false) * 100, 2)}%)
总内存 {Process.GetCurrentProcess().WorkingSet64.Bytes2MiB(2)}MiB";

    public static string Roll(string[] items)
        => items.Length < 2
            ? "没有选项让我怎么选，笨！"
            : string.Format(StringResources.RollMessage.RandomGet(), items.RandomGet());

    public static double Bytes2MiB(this long bytes, int round) => Math.Round(bytes / 1048576.0, round);
}
