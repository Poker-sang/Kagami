using Kagami.Attributes;
using Konata.Core.Message;
using System.ComponentModel;

namespace Kagami.Commands;

/// <summary>
/// 从Bilibili通过av、BV号获取视频信息
/// </summary>
public static class Bilibili
{
    [KagamiCmdlet(nameof(Av), CommandType = CmdletType.Prefix), Description("从Bilibili通过av号获取视频信息")]
    public static Task<MessageBuilder> Av(string av)
        => Services.Bilibili.GetVideoInfoFrom($"av{av}");

    [KagamiCmdlet("BV", CommandType = CmdletType.Prefix, IgnoreCase = false)]
    [Description("从Bilibili通过BV号获取视频信息")]
    public static Task<MessageBuilder> Bv(string bv)
        => Services.Bilibili.GetVideoInfoFrom($"BV{bv}");
}
