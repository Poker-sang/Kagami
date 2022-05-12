using Konata.Core.Message;

namespace Kagami.Commands;

/// <summary>
/// <inheritdoc/>
/// </summary>
public sealed class Ping : IKagamiCmdlet
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string Command { get; } = "ping";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string Description { get; } = "ping";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<MessageBuilder> InvokeAsync(Konata.Core.Bot? bot, Konata.Core.Events.Model.GroupMessageEvent? group, object[] args)
        => await Task.Run(() => Services.Kernel.Ping);
}
