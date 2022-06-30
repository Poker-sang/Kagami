using Kagami.UsedTypes;
using Konata.Core.Message;
using System.ComponentModel;

namespace Kagami.Commands;

public static class AcFun
{
    [Cmdlet(nameof(Ac), CmdletType = CmdletType.Prefix), Description("从AcFun通过ac号获取视频信息")]
    public static async ValueTask<MessageBuilder> Ac([Description("ac号")] uint ac)
        => await Services.AcFun.GetVideoInfoFrom($"ac{ac}");
}
