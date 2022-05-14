using Konata.Core.Message;

namespace Kagami.Commands;

/// <summary>
/// <inheritdoc/>
/// </summary>
public sealed class Roll : IKagamiCmdlet
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string Command { get; } = "roll";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string Description { get; } = "帮我选一个";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public (Type Type, string Description)[][] OverloadableArgumentList { get; } = { new []{
        (typeof(string[]), "选项")
    }};

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<MessageBuilder> InvokeAsync(Konata.Core.Bot? bot, Konata.Core.Events.Model.GroupMessageEvent? group, object[] args)
    {
        if (args[0] is not string[] options)
            return new(await StringResources.ArgumentErrorMessage.RandomGetAsync());
        return await Services.Kernel.RollAsync(options);
    }
}
