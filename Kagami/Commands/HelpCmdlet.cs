using System.ComponentModel;

using Kagami.Attributes;

using Konata.Core.Message;

namespace Kagami.Commands;
public static class HelpCmdlet
{
    [KagamiCmdlet(nameof(Help)), Description("获取帮助")]
    public static async Task<MessageBuilder> Help() => new MessageBuilder().Image(await Services.Help.GenerateImageAsync());
}
