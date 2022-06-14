using Kagami.ArgTypes;
using Kagami.Attributes;
using Konata.Core.Message;
using System.ComponentModel;

namespace Kagami.Commands;

public static class ArtificialIntelligence
{

    [Cmdlet(nameof(Ai)), Description("深度学习模型")]
    public static async Task<MessageBuilder> Ai(ArgTypes.Ai mode, Image image)
    {
        switch (mode)
        {
            case ArgTypes.Ai.Yolo:
                var stream = await Services.ArtificialIntelligence.Yolo(await image.Url.DownloadStreamAsync());
                var buffer = new byte[stream.Length];
                _ = await stream.ReadAsync(buffer);
                return new MessageBuilder().Image(buffer);
            default:
                throw new ArgumentOutOfRangeException(nameof(mode));
        }
    }
}
