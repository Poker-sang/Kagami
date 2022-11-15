using Kagami.ArgTypes;
using Kagami.UsedTypes;
using Konata.Core.Message;
using System.ComponentModel;
using static Kagami.Services.ArtificialIntelligence;

namespace Kagami.Commands;

public static class ArtificialIntelligence
{
    [Cmdlet(nameof(Ai)), Description("深度学习模型")]
    public static async Task<MessageBuilder> Ai(
        [Description("模型")] AiModel mode,
        [Description("图片")] Image image)
    {
        switch (mode)
        {
            case AiModel.Yolo:
            {
                await using var download = await image.Url.DownloadStreamAsync();
                await using var stream = await Yolo(download);
                var buffer = new byte[stream.Length];
                _ = await stream.ReadAsync(buffer);
                return new MessageBuilder().Image(buffer);
            }
            case AiModel.MobileNet:
            {
                await using var download = await image.Url.DownloadStreamAsync();
                var result = await MobileNet(download);
                return new MessageBuilder("一眼丁真，鉴定为：").TextLine(result);
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(mode));
        }
    }
}
