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
                var stream = await Yolo(await image.Url.DownloadStreamAsync());
                var buffer = new byte[stream.Length];
                _ = await stream.ReadAsync(buffer);
                return new MessageBuilder().Image(buffer);
            default:
                throw new ArgumentOutOfRangeException(nameof(mode));
        }
    }
}
