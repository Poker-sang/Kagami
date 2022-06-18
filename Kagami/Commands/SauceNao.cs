using Kagami.ArgTypes;
using Kagami.Attributes;
using Konata.Core.Message;
using System.ComponentModel;

namespace Kagami.Commands;

public static class SauceNao
{
    [Cmdlet(nameof(Sauce)), Description("从SauceNao搜索图源")]
    public static async ValueTask<MessageBuilder> Sauce(Image image)
        => new(await Services.SauceNao.Search(image.Url));
}
