using Konata.Core.Message;

namespace Kagami.Commands;

/// <summary>
/// 从Acfun通过ac号获取视频信息
/// </summary>
public sealed class Ac : IKagamiCmdlet
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string Command { get; } = "av";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string Description { get; } = "从AcFun通过ac号获取视频信息";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public (Type Type, string Description)[][] OverloadableArgumentList { get; } = { new []{
        (typeof(uint), "ac号")
    }};

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public Task<MessageBuilder> InvokeAsync(Konata.Core.Bot? bot, Konata.Core.Events.Model.GroupMessageEvent? group, object[] args)
        => Services.AcFun.GetVideoInfoFrom($"ac{args[0]}");

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public CommandType CommandType => CommandType.Prefix;
}
