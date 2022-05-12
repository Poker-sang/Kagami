using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kagami.ArgTypes;
using Konata.Core.Message;
using Konata.Core.Message.Model;

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
        (typeof(PicCommands), "来源")
    };

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<MessageBuilder> InvokeAsync(string[] args) => args[0] switch
    {
        "bing" => await Services.Bing.PictureAsync(),
        _ => new(await StringResources.ArgumentErrorMessage.RandomGetAsync()),
    };

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public CommandType CommandType => CommandType.Prefix;
}
