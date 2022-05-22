using Kagami.ArgTypes;
using Kagami.Attributes;
using Kagami.Core;
using Konata.Core.Message;
using System.ComponentModel;

namespace Kagami.Commands;

/// <summary>
/// 获取图片
/// </summary>
public static class Picture
{
    [Cmdlet(nameof(Pic)), Description("获取图片")]
    public static async ValueTask<MessageBuilder> Pic(PicSource command, string[]? args = null)
    {
        if (args is not null)
            args = args[2..];
        return command switch
        {
            PicSource.Bing => await Services.Bing.PictureAsync(),
            PicSource.Meme => args is { Length: 2 }
                ? await Services.PicMeme.PictureAsync(args[0], args[1])
                : new(StringResources.ArgumentErrorMessage.RandomGet()),
            _ => new(StringResources.ArgumentErrorMessage.RandomGet()),
        };
    }
}
