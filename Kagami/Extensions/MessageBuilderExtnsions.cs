using Konata.Core.Message;

namespace Kagami.Extensions;

public static class MessageBuilderExtensions
{
    /// <summary>
    /// �ڴ�����Ϣǰ����
    /// </summary>
    /// <param name="message"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    public static MessageBuilder TextLine(this MessageBuilder message, string text = "") => message.Text("\n" + text);
}
