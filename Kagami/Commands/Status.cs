using Konata.Core.Message;

namespace Kagami.Commands;

/// <summary>
/// <inheritdoc/>
/// </summary>
public sealed class Status : IKagamiCmdlet
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string Command { get; } = "status";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string Description { get; } = "查看状态";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<MessageBuilder> InvokeAsync(Konata.Core.Bot? bot, Konata.Core.Events.Model.GroupMessageEvent? group, object[] args)
        => await Task.Run(Services.Kernel.Status);
}
