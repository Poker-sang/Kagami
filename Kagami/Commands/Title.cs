using Konata.Core.Exceptions.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;

namespace Kagami.Commands;

/// <summary>
/// <inheritdoc/>
/// </summary>
public sealed class Title : IKagamiCmdlet
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string Command { get; } = "title";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string Description { get; } = "赋予头衔";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public (Type Type, string Description)[][] OverloadableArgumentList { get; } = { new []{
        (typeof(ArgTypes.At), "成员"),
        (typeof(string), "头衔")
    }};

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<MessageBuilder> InvokeAsync(Konata.Core.Bot bot, Konata.Core.Events.Model.GroupMessageEvent group, object[] args)
    {
        // Get at
        if (args[0] is not ArgTypes.At at)
            return new(await StringResources.ArgumentErrorMessage.RandomGetAsync());

        if (args[1] is not string title)
            return new(await StringResources.ArgumentErrorMessage.RandomGetAsync());

        try
        {
            if (await bot.GroupSetSpecialTitle(group.GroupUin, at.Uin, title.Trim(), uint.MaxValue))
                return new($"为 [{at.Uin}] 设置头衔");
            return new(await StringResources.UnknownErrorMessage.RandomGetAsync());
        }
        catch (OperationFailedException e)
        {
            Console.WriteLine($"{e.Message} ({e.HResult})");
            return new(await StringResources.OperationFailedMessage.RandomGetAsync());
        }
    }
}
