using Kagami.Services;
using Kagami.UsedTypes;
using Konata.Core.Message;
using System.ComponentModel;

namespace Kagami.Commands;

public static class Bilibili
{
    [Cmdlet(nameof(Av), CmdletType = CmdletType.Prefix), Description("从Bilibili通过av号获取视频信息")]
    public static async ValueTask<MessageBuilder> Av([Description("av号")] ulong av)
        => await Services.Bilibili.GetVideoInfoFrom($"av{av}");

    [Cmdlet("BV", CmdletType = CmdletType.Prefix, IgnoreCase = false)]
    [Description("从Bilibili通过BV号获取视频信息")]
    public static async ValueTask<MessageBuilder> Bv([Description("BV号")] string bv)
        => bv.Bv2Av() is { } av ? await Av(av) : new("不是正确的BV号");
}
