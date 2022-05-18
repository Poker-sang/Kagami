using Kagami.Attributes;
using Konata.Core.Message;
using System.ComponentModel;

namespace Kagami.Commands;
public static class HelpCmdlet
{
    [KagamiCmdlet(nameof(Help)), Description("获取帮助")]
    public static async ValueTask<MessageBuilder> Help() => new MessageBuilder().Image(await Services.Help.GenerateImageAsync());
}
