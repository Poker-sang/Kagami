using Konata.Core.Message;

namespace Kagami.Extensions;

public static class MessageBuilderExtensions
{
    /// <summary>
    /// 在此条消息前换行
    /// </summary>
    /// <param name="message"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public static MessageBuilder TextLine(this MessageBuilder message, string text = "") => message.Text("\n" + text);
}
