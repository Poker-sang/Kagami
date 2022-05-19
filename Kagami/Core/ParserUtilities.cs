using Kagami.ArgTypes;
using Kagami.Interfaces;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using System.Diagnostics;
using System.Text;

namespace Kagami.Core;
internal static class ParserUtilities
{
    internal static async Task<T> InvokeAsync<T>(this IKagamiReflectable reflectable, Bot bot, GroupMessageEvent group, params object?[]? parameters)
    {
        T? result = default;
        Task<T>? asyncResult = null;
        if (reflectable.ReturnType == typeof(T))
            result = (T)reflectable.Method(reflectable.Target, parameters)!;
        else if (reflectable.ReturnType == typeof(Task<T>))
            asyncResult = (Task<T>)reflectable.Method(reflectable.Target, parameters)!;
        else if (reflectable.ReturnType == typeof(ValueTask<T>))
            result = await (ValueTask<T>)reflectable.Method(reflectable.Target, parameters)!;

        if (asyncResult is not null)
        {
            // TODO 重构好看点
            if (reflectable is KagamiCmdlet)
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
    internal static bool ParseArguments(in IKagamiReflectable reflectable, in Bot bot, in GroupMessageEvent group, in string raw, in string[] args, out object?[]? parameters)
    {
        parameters = null;

        // 这个变量表示不计入参数数量的参数个数
        byte appendArgCount = 0;
        if (reflectable.Parameters.Any(i => i.Type == typeof(Bot)))
            appendArgCount++;

        if (reflectable.Parameters.Any(i => i.Type == typeof(GroupMessageEvent)))
            appendArgCount++;

        if (reflectable.Parameters.Any(i => i.Type == typeof(Raw)))
            appendArgCount++;

        var minArgCount = reflectable.Parameters.Length - appendArgCount - reflectable.Parameters
            .Where(i => i.HasDefault).Count();

        // 断言Cmdlet最少参数数量比传入参数数量多
        if (args.Length < minArgCount)
            return false;

        // 断言Cmdlet最多参数数量比传入参数数量少
        //if (args.Length > reflectable.Parameters.Length - appendArgCount)
        //    return false;

        var arguments = new List<object?>(reflectable.Parameters.Length);
        TypeParser.Clear();
        var argsList = args.ToList();
        for (ushort i = 0;
            i < reflectable.Parameters.Length && i < args.Length + appendArgCount;
            ++i)
        {
            var parameter = reflectable.Parameters[i];

            var type = parameter.Type;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = type.GenericTypeArguments[0];

            if ((parameter.Type == typeof(Bot)
                || parameter.Type == typeof(GroupMessageEvent)
                || parameter.Type == typeof(Raw))
                && TypeParser.Map[parameter.Type](bot, group, raw, out var obj))
                arguments.Add(obj);
            else if (!TypeParser.Map.TryGetValue(type, out var parser)) // 获取解析器
                throw new NotSupportedException($"类型解析器器不支持的类型 \"{type.FullName}\". ");
            else
            {
                var flag = false;
                foreach (var arg in argsList)
                {
                    if (parser(bot, group, arg, out obj)) // 解析字符串
                    {
                        arguments.Add(obj);
                        _ = argsList.Remove(arg);
                        flag = true;
                        break;
                    }
                }

                if (!flag)
                    return false;
            }
            //else if (parameter.HasDefault) // failback使用默认值
            //    arguments.Add(parameter.Default);
            // 失败
        }

        if (arguments.Count < reflectable.Parameters.Length)
            for (var i = arguments.Count; i < reflectable.Parameters.Length; i++)
            {
                var parameter = reflectable.Parameters[i];
                if ((parameter.Type == typeof(Bot) || parameter.Type == typeof(GroupMessageEvent)) && TypeParser.Map[parameter.Type](bot, group, "", out var obj))
                {
                    arguments.Add(obj);
                    continue;
                }

                if (!parameter.HasDefault)
                    return false;
                arguments.Add(parameter.Default);
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

        if (string.IsNullOrWhiteSpace(raw))
            throw new ArgumentException($"\"{nameof(raw)}\" 不能为 null 或空白。", nameof(raw));

        var result = new List<string>();
        var sb = new StringBuilder();
        var quotes = new Stack<char>();
        foreach (var ch in raw)
            switch (ch)
            {
                case '"':
                case '\'':
                    if (quotes.TryPeek(out var tmp) && tmp == ch)
                        _ = quotes.Pop();
                    else
                        quotes.Push(ch);
                    break;

                case ' ':
                    if (quotes.Count is 0)
                    {
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
            throw new FormatException("输入的格式不正确");

        result.Add(sb.ToString());
        _ = sb.Clear();

        result.ForEach(i => Debug.WriteLine($"[arg]: {i}"));

        return result.ToArray();
    }
}
