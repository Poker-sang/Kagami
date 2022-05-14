﻿using Kagami.ArgTypes;
using Konata.Core.Message;

namespace Kagami.Commands;

/// <summary>
/// 获取图片
/// </summary>
public sealed class Pic : IKagamiCmdlet
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
    public (Type Type, string Description)[][] OverloadableArgumentList { get; } = { new []{
        (typeof(PicCommands), "来源")
    }};

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public async Task<MessageBuilder> InvokeAsync(Konata.Core.Bot? bot, Konata.Core.Events.Model.GroupMessageEvent? group, object[] args)
        => (PicCommands)args[0] switch
        {
            PicCommands.Bing => await Services.Bing.PictureAsync(),
            _ => new(await StringResources.ArgumentErrorMessage.RandomGetAsync()),
        };
}
