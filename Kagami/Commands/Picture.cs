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
    public static async ValueTask<MessageBuilder> Pic(PicSource command)
        => command switch
        {
            PicSource.Bing => await Services.Bing.PictureAsync(),
            _ => new(StringResources.ArgumentErrorMessage.RandomGet()),
        };
}
