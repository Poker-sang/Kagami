using System;
using System.Collections.Generic;
using Konata.Core.Message;

namespace Kagami.Function;

public static partial class Commands
{
    private static uint _messageCounter;

    public static MessageBuilder Text(string text) => new(text);
    private static MessageBuilder TextLine(this MessageBuilder message, string text = "") => message.Text("\n" + text);

    private static string ArgumentError => ArgumentErrorMessage.RandomGet();
    private static readonly string[] ArgumentErrorMessage = { "参数不对哦", "请再检查一下参数", "没听懂欸", "嗯？" };
    private static string UnknownError => UnknownErrorMessage.RandomGet();
    private static readonly string[] UnknownErrorMessage = { "咱也不知道出了什么问题", "呜呜失败了" };
    private static string OperationFailed => OperationFailedMessage.RandomGet();
    private static readonly string[] OperationFailedMessage = { "操作失败了..." };

    public static T RandomGet<T>(this IReadOnlyList<T> array) => array[new Random().Next(array.Count)];
}