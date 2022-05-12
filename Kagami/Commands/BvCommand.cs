using Konata.Core.Message;
using Konata.Core.Message.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kagami.Commands;

/// <summary>
/// 从Bilibili通过BV号获取视频信息
/// </summary>
public sealed class BvCommand : IKagamiCommand
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string Command { get; } = "BV";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string Description { get; } = "从Bilibili通过BV号获取视频信息";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public (Type, string)[] Arguments { get; } = new[] {
        (typeof(string), "BV号")
    };

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public Task<MessageBuilder> InvokeAsync(string[] args)
        => Services.Bilibili.GetVideoInfoFrom($"BV{args[0]}");

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public CommandType CommandType => CommandType.Prefix;

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public bool IgnoreCase => false;
}
