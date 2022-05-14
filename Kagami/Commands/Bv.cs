﻿using Konata.Core.Message;

namespace Kagami.Commands;

/// <summary>
/// 从Bilibili通过BV号获取视频信息
/// </summary>
public sealed class Bv : IKagamiCmdlet
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
    public (Type Type, string Description)[][] OverloadableArgumentList { get; } = { new []{
        (typeof(string), "BV号")
    }};

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public Task<MessageBuilder> InvokeAsync(Konata.Core.Bot? bot, Konata.Core.Events.Model.GroupMessageEvent? group, object[] args)
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
