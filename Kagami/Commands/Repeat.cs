using Kagami.Utils;
using Konata.Core.Exceptions.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Konata.Core.Message.Model;

namespace Kagami.Commands;

/// <summary>
/// <inheritdoc/>
/// </summary>
public sealed class Repeat : IKagamiCmdlet
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string Command { get; } = "repeat";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string Description { get; } = "复读机";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public (Type Type, string Description)[][] OverloadableArgumentList{ get; } = { new []{
        (typeof(MessageChain), "内容")
    }};

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<MessageBuilder> InvokeAsync(Konata.Core.Bot? bot, Konata.Core.Events.Model.GroupMessageEvent? group, object[] args)
    {
        if (args[0] is not MessageChain message)
            return new(await StringResources.ArgumentErrorMessage.RandomGetAsync());
        return Services.Kernel.Repeat(message);
    }
}
