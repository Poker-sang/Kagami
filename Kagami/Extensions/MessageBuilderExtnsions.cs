using Konata.Core.Message;

namespace Kagami.Extensions;

public static class MessageBuilderExtensions
{
    public static MessageBuilder TextLine(this MessageBuilder message, string text = "") => message.Text("\n" + text);
}
