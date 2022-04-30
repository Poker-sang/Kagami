using Konata.Core.Message;

namespace Kagami.Function;

public partial class Commands
{
    private static uint _messageCounter;

    public static MessageBuilder Text(string text) => new MessageBuilder().Text(text);
}