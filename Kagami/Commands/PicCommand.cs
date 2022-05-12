using Konata.Core.Message;

namespace Kagami.Commands;

/// <summary>
/// 获取图片
/// </summary>
public sealed class PicCommand : IKagamiCommand
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string Command { get; } = "pic";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public string Description { get; } = "从指定来源获取图片";

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public (Type, string)[] Arguments { get; } = new[] {
        (typeof(ArgTypes.PicCommands), "来源")
    };

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<MessageBuilder> InvokeAsync(Konata.Core.Bot? bot, Konata.Core.Events.Model.GroupMessageEvent? group, object[] args) => (ArgTypes.PicCommands)args[0] switch
    {
        ArgTypes.PicCommands.Bing => await Services.Bing.PictureAsync(),
        _ => new(await StringResources.ArgumentErrorMessage.RandomGetAsync()),
    };
}
