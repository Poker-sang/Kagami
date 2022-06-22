using Kagami.Attributes;
using Kagami.Enums;
using Konata.Core.Message;
using System.ComponentModel;

namespace Kagami.Commands;

/// <summary>
/// 从Bilibili通过av、BV号获取视频信息
/// </summary>
public static class Bilibili
{
    [Cmdlet(nameof(Av), CmdletType = CmdletType.Prefix), Description("从Bilibili通过av号获取视频信息")]
    public static async ValueTask<MessageBuilder> Av([Description("av号")] uint av)
        => await Services.Bilibili.GetVideoInfoFrom($"av{av}");

    [Cmdlet("BV", CmdletType = CmdletType.Prefix, IgnoreCase = false)]
    [Description("从Bilibili通过BV号获取视频信息")]
    public static async ValueTask<MessageBuilder> Bv([Description("BV号")] string bv)
        => await Services.Bilibili.GetVideoInfoFrom($"BV{bv}");
}
