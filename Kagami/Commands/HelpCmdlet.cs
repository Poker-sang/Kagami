using Kagami.UsedTypes;
using Konata.Core.Message;
using System.ComponentModel;

namespace Kagami.Commands;
public static class HelpCmdlet
{
    [Cmdlet(nameof(Help)), Description("获取帮助")]
    public static async ValueTask<MessageBuilder> Help() => new MessageBuilder().Image(await Services.Help.GenerateImageAsync());
}
