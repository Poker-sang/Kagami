using Kagami.Utils;
using Konata.Core.Exceptions.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Konata.Core.Message.Model;

namespace Kagami.Commands;

/// <summary>
/// <inheritdoc/>
/// </summary>
public sealed class Mute : IKagamiCmdlet
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string Command { get; } = "mute";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string Description { get; } = "禁言成员";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public (Type Type, string Description)[][] OverloadableArgumentList{ get; } = { new []{
        (typeof(ArgTypes.At), "成员"),
        (typeof(uint), "时间(分钟)")
    }};

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public int ArgumentCount => 1;

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

        // Parse time
        var time = 10U;
        if (args.Length > 1)
            time = (uint)args[1];

        try
        {
            if (await bot.GroupMuteMember(group.GroupUin, at.Uin, time * 60))
                return new($"禁言 [{at.Uin}] {time}分钟");
            return new(await StringResources.UnknownErrorMessage.RandomGetAsync());
        }
        catch (OperationFailedException e)
        {
            Console.WriteLine(e.Message);
            return new(await StringResources.OperationFailedMessage.RandomGetAsync());
        }
    }
}
