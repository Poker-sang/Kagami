using Kagami.Utils;
using Konata.Core.Exceptions.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Konata.Core.Message.Model;

namespace Kagami.Commands;

/// <summary>
/// <inheritdoc/>
/// </summary>
public sealed class TitleCommand : IKagamiCommand
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
    public (Type, string)[] Arguments { get; } = new[] {
        (typeof(ArgTypes.At), "成员"),
        (typeof(string), "头衔")
    };

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<MessageBuilder> InvokeAsync(Konata.Core.Bot? bot, Konata.Core.Events.Model.GroupMessageEvent? group, object[] args)
    {
        Assert.IsNotNull<ArgumentException>(bot, "内部错误 - bot不能为空", nameof(bot));
        Assert.IsNotNull<ArgumentException>(group, "内部错误 - group不能为空", nameof(group));

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
