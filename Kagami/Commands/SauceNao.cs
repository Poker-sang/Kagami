using Kagami.ArgTypes;
using Kagami.UsedTypes;
using Konata.Core.Message;
using System.ComponentModel;

namespace Kagami.Commands;

public static class SauceNao
{
    [Obsolete("连不上")]
    [Cmdlet(nameof(Sauce), "搜图"), Description("从SauceNao搜索图源")]
    public static async ValueTask<MessageBuilder> Sauce(Image image)
        => new(await Services.SauceNao.Search(image.Url));
}
