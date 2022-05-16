using Kagami.Attributes;
using Kagami.Utils;
using Konata.Core;
using Konata.Core.Common;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Konata.Core.Message.Model;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace Kagami;

internal record class KagamiCmdletParameter(Type Type, string Name, bool HasDefault, object? Default, string Description);
internal record class KagamiCmdlet(
    string Name,
    RoleType Permission,
    CmdletType CommandType,
    bool IgnoreCase,
    KagamiCmdletParameter[] Parameters,
    string Description,
    object? Target,
    Type ReturnType,
    Func<object?, object?[]?, object?> Method);

/// <summary>
/// 入口
/// </summary>
public static class Entry
{
    public static void Initialize()
    {
        var types = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .SelectMany(asm => asm.GetTypes());

        // 不要标注KagamiCmdletClassAttribute了
        //.Where(t => t.CustomAttributes.Any(i => i.AttributeType == typeof(KagamiCmdletClassAttribute)));
        List<KagamiCmdlet> cmdlets = new();
        foreach (var type in types)
        {
            object? target = null;

            foreach (var method in type.GetMethods())
            {
                // 没有标注是命令的
                if (method.GetCustomAttribute<KagamiCmdletAttribute>() is not KagamiCmdletAttribute attribute)
                    continue;

                // 方法类型不对的
                if (!method.ReturnType.IsAssignableFrom(typeof(MessageBuilder))
                    && !method.ReturnType.IsAssignableFrom(typeof(Task<MessageBuilder>))
                    && !method.ReturnType.IsAssignableFrom(typeof(ValueTask<MessageBuilder>)))
                {
                    Console.Error.WriteLine($"警告: 方法\"[{type.FullName}]::{method.Name}()\"的返回类型不正确, 将忽略这个命令!");
                    continue;
                }

                var parameters = method
                        .GetParameters()
                        .Select(parameter => new KagamiCmdletParameter(
                            parameter.ParameterType,
                            parameter.Name!,
                            parameter.HasDefaultValue,
                            parameter.DefaultValue,
                            parameter.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty))
                        .ToArray();


                // 静态类的修饰符是abstract sealed
                // 它不是抽象类
                if (!type.IsAbstract && target is null)
                    target = Activator.CreateInstance(type);

                KagamiCmdlet cmdlet = new(
                    attribute.Name,
                    attribute.Permission,
                    attribute.CommandType,
                    attribute.IgnoreCase,
                    parameters,
                    method.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty,
                    target,
                    method.ReturnType,
                    method.Invoke);

                Console.WriteLine($"发现Cmdlet: {cmdlet.Permission} {cmdlet.CommandType} {cmdlet.ReturnType} {(cmdlet.IgnoreCase ? string.Empty : "*")}{cmdlet.Name}({string.Join(", ", cmdlet.Parameters.Select(i => $"{i.Type} {i.Name}{(i.HasDefault ? $" = {i.Default}" : string.Empty)}"))})");

                cmdlets.Add(cmdlet);
            }
        }
        Cmdlets = cmdlets.GroupBy(i => i.CommandType)
            .ToDictionary(
            i => i.Key,
            i => new HashSet<KagamiCmdlet>(i));
    }

    /// <summary>
    /// 通过反射获取所有可用命令
    /// </summary>
    internal static Dictionary<CmdletType, HashSet<KagamiCmdlet>>? Cmdlets { get; private set; }

    /// <summary>
    /// 给机器人挂事件的入口
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    public static async void ParseCommand(Bot bot, GroupMessageEvent group)
    {
        var value = group.Message.Chain[0] is TextChain textChain ? await ParseCommand(textChain.Content, bot, group) : null;
        if (value is not null)
            _ = await bot.SendGroupMessage(group.GroupUin, value);
    }

    /// <summary>
    /// 解析命令
    /// </summary>
    /// <param name="raw">原始字符串</param>
    /// <returns></returns>
    public static async Task<MessageBuilder?> ParseCommand(string raw, Bot bot, GroupMessageEvent group)
    {
        MessageBuilder? result = null;
        try
        {
            if (Cmdlets is null) // 当不小心忘记初始化时执行
                Initialize();

            if (string.IsNullOrWhiteSpace(raw))
                throw new ArgumentException($"\"{nameof(raw)}\" 不能为 null 或空白。", nameof(raw));

            string[] args = SplitCommand(raw);
            string cmd = args[0]; // 获取第一个元素用作命令
            Assert.ThrowIf<InvalidOperationException>(cmd.Contains(' '), "命令名中不能包括空格");

            if (Cmdlets!.TryGetValue(CmdletType.Normal, out var set))
                result = await ParseCommand(cmd, args[1..], set, bot, group, string.Equals);
            if (result is null && Cmdlets.TryGetValue(CmdletType.Prefix, out set))
                result = await ParseCommand(cmd, args, set, bot, group, (i, o, s) => i?.StartsWith(o, s) ?? false, true);
            if (result is null)
                throw new NotSupportedException("不支持的Cmdlet类型");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.ToString());
        }
        return result;
    }

    /// <summary>
    /// 先进字符串分割
    /// </summary>
    /// <param name="raw">原始字符串</param>
    /// <returns>分割后的字符串数组</returns>
    /// <exception cref="FormatException">引号栈不平衡时引发的异常</exception>
    internal static string[] SplitCommand(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new ArgumentException($"\"{nameof(raw)}\" 不能为 null 或空白。", nameof(raw));

        List<string> result = new();
        StringBuilder sb = new();
        Stack<char> quotes = new();
        foreach (var ch in raw)
        {
            switch (ch)
            {
                case '"':
                case '\'':
                    if (quotes.TryPeek(out var tmp) && tmp == ch)
                        quotes.Pop();
                    else
                        quotes.Push(ch);
                    break;

                case ' ':
                    if (quotes.Count is 0)
                    {
                        if (sb.Length > 0)
                            result.Add(sb.ToString());
                        sb.Clear();
                    }
                    else
                    {
                        sb.Append(ch);
                    }
                    break;

                default:
                    sb.Append(ch);
                    break;
            }
        }
        Assert.ThrowIfNot<FormatException>(quotes.Count is 0, "输入的格式不正确");

        result.Add(sb.ToString());
        sb.Clear();

        return result.ToArray();
    }

    private static async Task<MessageBuilder> InvokeCommandAsync(this KagamiCmdlet cmdlet, Bot bot, GroupMessageEvent group, params object?[]? parameters)
    {
        MessageBuilder? result = null;
        Task<MessageBuilder>? asyncResult = null;
        if (cmdlet.ReturnType == typeof(MessageBuilder))
            result = (MessageBuilder)cmdlet.Method(cmdlet.Target, parameters)!;
        else if (cmdlet.ReturnType == typeof(Task<MessageBuilder>))
            asyncResult = (Task<MessageBuilder>)cmdlet.Method(cmdlet.Target, parameters)!;
        else if (cmdlet.ReturnType == typeof(ValueTask<MessageBuilder>))
            result = await (ValueTask<MessageBuilder>)cmdlet.Method(cmdlet.Target, parameters)!;

        if (asyncResult is not null)
        {
            _ = bot.SendGroupMessage(group.GroupUin, "正在执行, 请稍候...").ConfigureAwait(false);
            result = await asyncResult;
        }

        Assert.IsNotNull<InvalidOperationException>(result, "命令返回的类型不正确");

        return result;
    }
    /// <summary>
    /// 参数解析器
    /// </summary>
    /// <param name="cmdlet">命令</param>
    /// <param name="bot">机器人实例</param>
    /// <param name="group">群消息事件实例</param>
    /// <param name="args">传入的字符串参数</param>
    /// <param name="parameters">解析后的对象参数</param>
    /// <returns>是否成功</returns>
    /// <exception cref="NotSupportedException">类型解析器不支持的类型</exception>
    private static bool ParseArguments(in KagamiCmdlet cmdlet, in Bot bot, in GroupMessageEvent group, in string[] args, out object?[]? parameters)
    {
        parameters = null;
        int minArgCount = cmdlet.Parameters.Where(i => i.Default is null).Count();

        // 断言Cmdlet最少参数数量比传入参数数量多
        if (args.Length < minArgCount)
            return false;

        List<object?> arguments = new(cmdlet.Parameters.Length);
        TypeParser.Clear();
        for (int i = 0; i < cmdlet.Parameters.Length && i < args.Length; i++)
        {
            KagamiCmdletParameter parameter = cmdlet.Parameters[i];
            if (!TypeParser.Map.TryGetValue(parameter.Type, out var parser)) // 获取解析器
                throw new NotSupportedException($"类型解析器器不支持的类型 \"{parameter.Type.FullName}\". ");
            else if (parser(bot, group, args[i], out object? obj)) // 解析字符串
                arguments.Add(obj);
            else if (parameter.HasDefault) // failback使用默认值
                arguments.Add(parameter.Default);
            else // 失败
                return false;
        }

        if (arguments.Count < cmdlet.Parameters.Length)
        {
            for (int i = arguments.Count; i < cmdlet.Parameters.Length; i++)
            {
                KagamiCmdletParameter parameter = cmdlet.Parameters[i];
                if (!parameter.HasDefault)
                    return false;

                arguments.Add(parameter.Default);
            }
        }

        parameters = arguments.ToArray();
        return true;
    }

    /// <summary>
    /// 解析命令
    /// </summary>
    /// <param name="cmd">命令名</param>
    /// <param name="args">参数</param>
    /// <param name="set">Cmdlet集</param>
    /// <param name="bot">机器人实例</param>
    /// <param name="group">群消息事件实例</param>
    /// <param name="matcher">命令匹配器</param>
    /// <param name="skipPrefix">解析参数时, 首个参数是否需要跳过命令前缀长度个字符</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">命令名不能为空</exception>
    /// <exception cref="InvalidOperationException">找不到合适的命令</exception>
    /// <exception cref="NotSupportedException">类型解析器不支持的类型</exception>
    private static async Task<MessageBuilder?> ParseCommand(
        string cmd,
        string[] args,
        HashSet<KagamiCmdlet> set,
        Bot bot,
        GroupMessageEvent group,
        Func<string?, string, StringComparison, bool> matcher,
        bool skipPrefix = false)
    {
        if (string.IsNullOrEmpty(cmd))
            throw new ArgumentException($"\"{nameof(cmd)}\" 不能为 null 或空。", nameof(cmd));

        var cmdset = set.Where(i => matcher(cmd, i.Name, i.IgnoreCase
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal))
            .OrderBy(i => i.Parameters.Length) // 按参数数量从小到大排序
            .ToArray();

        if (cmdset.Length is 0)
            return null; // 找不到匹配的命令

        foreach (var cmdlet in cmdset)
        {
            if (skipPrefix)
                args[0] = args[0][cmdlet.Name.Length..];
            if (ParseArguments(cmdlet, bot, group, args, out var parameters))
                return await cmdlet.InvokeCommandAsync(bot, group, parameters);
        }
        throw new InvalidOperationException("找不到合适的cmdlet重载");
    }
}
