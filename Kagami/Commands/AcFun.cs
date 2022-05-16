﻿using Kagami.Attributes;
using Konata.Core.Message;
using System.ComponentModel;

namespace Kagami.Commands;

/// <summary>
/// 从Acfun通过ac号获取视频信息
/// </summary>
public static class AcFun
{
    [KagamiCmdlet(nameof(Ac), CommandType = CmdletType.Prefix), Description("从AcFun通过ac号获取视频信息")]
    public static async Task<MessageBuilder> Ac(uint ac)
        => await Services.AcFun.GetVideoInfoFrom($"ac{ac}");
}