using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Konata.Core.Message;
using Konata.Core.Message.Model;

namespace Kagami.Commands;

/// <summary>
/// 从Bilibili通过av号获取视频信息
/// </summary>
public sealed class Av : IKagamiCmdlet
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string Command { get; } = "av";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string Description { get; } = "从Bilibili通过av号获取视频信息";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public (Type Type, string Description)[][] OverloadableArgumentList{ get; } = { new []{
        (typeof(uint), "av号")
    }};

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public Task<MessageBuilder> InvokeAsync(Konata.Core.Bot? bot, Konata.Core.Events.Model.GroupMessageEvent? group, object[] args)
        => Services.Bilibili.GetVideoInfoFrom($"av{args[0]}");

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public CommandType CommandType => CommandType.Prefix;
}
