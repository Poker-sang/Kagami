using Kagami.ArgTypes;
using Kagami.Attributes;
using Kagami.Interfaces;
using Kagami.Records;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using System.Diagnostics;
using System.Text;

namespace Kagami.Core;
internal static class ParserUtilities
{
    internal static async Task<T> InvokeAsync<T, TAttribute>(this Record<TAttribute> reflectable, Bot bot, GroupMessageEvent group, params object?[]? parameters)
        where TAttribute : Attribute, IKagamiPermission
    {
        T? result = default;
        Task<T>? asyncResult = null;
        if (reflectable.Method.ReturnType == typeof(T))
            result = (T)reflectable.Method.Invoke(null, parameters)!;
        else if (reflectable.Method.ReturnType == typeof(Task<T>))
            asyncResult = (Task<T>)reflectable.Method.Invoke(null, parameters)!;
        else if (reflectable.Method.ReturnType == typeof(ValueTask<T>))
            result = await (ValueTask<T>)reflectable.Method.Invoke(null, parameters)!;

        if (asyncResult is not null)
        {
            if (!asyncResult.IsCompleted)
                // TODO 重构好看点
                if (typeof(CmdletAttribute) == typeof(TAttribute))
                    _ = bot.SendGroupMessage(group.GroupUin, StringResources.ProcessingMessage.RandomGet()).ConfigureAwait(false);
            result = await asyncResult;
        }

        return result is null ? throw new InvalidOperationException("命令返回的类型不正确") : result;
    }

    /// <summary>
    /// 参数解析器
    /// </summary>
    /// <param name="reflectable">命令</param>
    /// <param name="bot">机器人实例</param>
    /// <param name="group">群消息事件实例</param>
    /// <param name="raw">传入的原字符串，目前只用于<see cref="Raw"/>参数</param>
    /// <param name="args">传入的字符串参数</param>
    /// <param name="parameters">解析后的对象参数</param>
    /// <returns>是否成功</returns>
    /// <exception cref="NotSupportedException">类型解析器不支持的类型</exception>
    internal static bool ParseArguments<T>(
        this Record<T> reflectable,
        in Bot bot, in GroupMessageEvent group, in Raw raw,
        in string[] args, out object?[]? parameters) where T : Attribute, IKagamiPermission
    {
        parameters = null;

        var arguments = new List<object?>(reflectable.Parameters.Length);
        TypeParser.Clear();
        var argsList = args.ToList();

        foreach (var parameter in reflectable.Parameters)
        {
            var type = parameter.Type;
            var hasDefault = false;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = type.GenericTypeArguments[0];

            if (parameter.HasDefault)
                hasDefault = true;


            if (parameter.Type == typeof(Bot))
                arguments.Add(bot);
            else if (parameter.Type == typeof(GroupMessageEvent))
                arguments.Add(group);
            else if (parameter.Type == typeof(Raw))
                arguments.Add(raw);
            else if (TypeParser.Map.TryGetValue(type, out var parser))
            {
                var flag = false;
                foreach (var arg in argsList)
                    // 解析字符串
                    if (parser(bot, group, arg) is { } obj)
                    {
                        arguments.Add(obj);
                        _ = argsList.Remove(arg);
                        flag = true;
                        break;
                    }
                if (!flag)
                    if (hasDefault)
                        arguments.Add(Type.Missing);
                    else
                        return false;
            }
            else
                throw new NotSupportedException($"类型解析器器不支持的类型 \"{type.FullName}\". ");
        }

        parameters = arguments.ToArray();
        return true;
    }

    /// <summary>
    /// 先进字符串分割
    /// </summary>
    /// <param name="raw">原始字符串</param>
    /// <returns>分割后的字符串数组</returns>
    /// <exception cref="FormatException">引号栈不平衡时引发的异常</exception>
    internal static string[] SplitRawString(this string raw)
    {
        Debug.WriteLine(raw);

        if (raw is "")
        {
            Console.Error.WriteLine($"\"{nameof(raw)}\" 不能为 null 或空白。");
            return Array.Empty<string>();
        }

        // 参数结果
        var result = new List<string>();
        // 临时记录当前arg
        var sb = new StringBuilder();
        // 括号栈
        var quotes = new Stack<char>();
        foreach (var ch in raw)
            switch (ch)
            {
                case '"':
                case '\'':
                    // 如果连续两个相同，则弹出
                    if (quotes.TryPeek(out var tmp) && tmp == ch)
                        _ = quotes.Pop();
                    //不同则入栈
                    else
                        quotes.Push(ch);
                    break;

                case ' ':
                    // 如果括号栈为空
                    if (quotes.Count is 0)
                    {
                        // 如果已经记录了参数，则加入结果
                        if (sb.Length > 0)
                            result.Add(sb.ToString());
                        _ = sb.Clear();
                    }
                    else
                        _ = sb.Append(ch);
                    break;
                default:
                    _ = sb.Append(ch);
                    break;
            }

        if (quotes.Count is not 0)
        {
            Console.Error.WriteLine("输入的格式不正确");
            return Array.Empty<string>();
        }

        result.Add(sb.ToString());
        _ = sb.Clear();

        result.ForEach(i => Debug.WriteLine($"[arg]: {i}"));

        return result.ToArray();
    }
}
