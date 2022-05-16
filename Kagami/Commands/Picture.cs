using Kagami.ArgTypes;
using Kagami.Attributes;
using Konata.Core.Message;
using System.ComponentModel;

namespace Kagami.Commands;

/// <summary>
/// 获取图片
/// </summary>
public static class Picture
{
    [KagamiCmdlet(nameof(Pic)), Description("获取图片")]
    public static async Task<MessageBuilder> Pic(PicCommands command)
        => command switch
        {
            PicCommands.Bing => await Services.Bing.PictureAsync(),
            _ => new(await StringResources.ArgumentErrorMessage.RandomGetAsync()),
        };
}
