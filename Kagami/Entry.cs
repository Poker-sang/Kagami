using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;

using Kagami.Attributes;

using Konata.Core;
using Konata.Core.Common;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using Konata.Core.Message.Model;

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

    private static volatile nuint s_messageCounter = 0;
    public static nuint MessageCounter => s_messageCounter;

    static Entry()
    {
        IEnumerable<Type>? types = AppDomain
            .CurrentDomain
            .GetAssemblies()
            .SelectMany(asm => asm.GetTypes());

        // 不要标注KagamiCmdletClassAttribute了
        //.Where(t => t.CustomAttributes.Any(i => i.AttributeType == typeof(KagamiCmdletClassAttribute)));
        List<KagamiCmdlet> cmdlets = new();
        foreach (Type? type in types)
        {
            object? target = null;

            foreach (MethodInfo? method in type.GetMethods())
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

                KagamiCmdletParameter[]? parameters = method
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
    internal static Dictionary<CmdletType, HashSet<KagamiCmdlet>> Cmdlets { get; }

    /// <summary>
    /// 给机器人挂事件的入口
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    public static async void ParseCommand(Bot bot, GroupMessageEvent group)
    {
        Console.WriteLine($"[\x1b[38;2;0;255;255m{DateTime.Now:T}\u001b[0m]  [\x1b[38;2;0;0;255m{group.GroupName}/\x1b[38;2;0;255;0m{group.MemberCard.Replace("\x7", string.Empty)}\u001b[0m] : {group.Chain}\u001b[0m");

        if (group.MemberUin == bot.Uin)
            return;

        if (!group.Message.Chain.Any())
            return;

        StringBuilder sb = new();
        foreach (BaseChain chain in group.Message.Chain)
        {
            switch (chain.Type)
            {
                case BaseChain.ChainType.At:
                case BaseChain.ChainType.Reply:
                case BaseChain.ChainType.Image:
                case BaseChain.ChainType.Flash:
                case BaseChain.ChainType.Record:
                case BaseChain.ChainType.Video:
                case BaseChain.ChainType.QFace:
                case BaseChain.ChainType.BFace:
                case BaseChain.ChainType.Xml:
                case BaseChain.ChainType.MultiMsg:
                case BaseChain.ChainType.Json:
                    sb.Append(@$" '<placeholder type=""{chain.Type}""/>' ");
                    break;
                case BaseChain.ChainType.Text:
                    sb.Append(chain.As<TextChain>()?.Content);
                    break;
                default:
                    break;
            }
        }

        MessageBuilder? value = await ParseCommand(sb.ToString().Trim(), bot, group);

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
            if (string.IsNullOrWhiteSpace(raw))
                throw new ArgumentException($"\"{nameof(raw)}\" 不能为 null 或空白。", nameof(raw));

            string[]? args = SplitCommand(raw);
            string? cmd = args[0].Trim(); // 获取第一个元素用作命令
            if (cmd.Contains(' '))
                throw new InvalidOperationException("命令名中不能包括空格");

            if (Cmdlets.TryGetValue(CmdletType.Normal, out HashSet<KagamiCmdlet>? set))
                result = await ParseCommand(cmd, args[1..], set, bot, group, string.Equals);
            if (result is null && Cmdlets.TryGetValue(CmdletType.Prefix, out set))
                result = await ParseCommand(cmd, args, set, bot, group, (i, o, s) => i?.StartsWith(o, s) ?? false, true);
            if (result is null)
                throw new NotSupportedException("不支持的Cmdlet类型");
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
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
        Debug.WriteLine(raw);

        if (string.IsNullOrWhiteSpace(raw))
            throw new ArgumentException($"\"{nameof(raw)}\" 不能为 null 或空白。", nameof(raw));

        List<string> result = new();
        StringBuilder sb = new();
        Stack<char> quotes = new();
        foreach (char ch in raw)
        {
            switch (ch)
            {
                case '"':
                case '\'':
                    if (quotes.TryPeek(out char tmp) && tmp == ch)
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
                    {
                        _ = sb.Append(ch);
                    }

                    break;

                default:
                    _ = sb.Append(ch);
                    break;
            }
        }

        if (quotes.Count is not 0)
            throw new FormatException("输入的格式不正确");

        result.Add(sb.ToString());
        _ = sb.Clear();

        result.ForEach(i => Debug.WriteLine($"[arg]: {i}"));

        return result.ToArray();
    }

    private static async Task<MessageBuilder> InvokeCommandAsync(this KagamiCmdlet cmdlet, Bot bot, GroupMessageEvent group, params object?[]? parameters)
    {
        s_messageCounter++;
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
            _ = bot.SendGroupMessage(group.GroupUin, StringResources.ProcessingMessage.RandomGet()).ConfigureAwait(false);
            result = await asyncResult;
        }

        if (result is null)
            throw new InvalidOperationException("命令返回的类型不正确");

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

        // 这个变量表示不计入参数数量的参数个数
        byte appendArgCount = 0;
        if (cmdlet.Parameters.Any(i => i.Type == typeof(Bot)))
            appendArgCount++;
        if (cmdlet.Parameters.Any(i => i.Type == typeof(GroupMessageEvent)))
            appendArgCount++;

        int minArgCount = cmdlet.Parameters.Length - appendArgCount - cmdlet.Parameters
            .Where(i => i.HasDefault)
            .Count();

        // 断言Cmdlet最少参数数量比传入参数数量多
        if (args.Length < minArgCount)
            return false;

        // 断言Cmdlet最多参数数量比传入参数数量少
        if (args.Length > cmdlet.Parameters.Length - appendArgCount)
            return false;

        List<object?> arguments = new(cmdlet.Parameters.Length);
        TypeParser.Clear();
        for (ushort i = 0, p = 0;
            i < cmdlet.Parameters.Length && i < args.Length + appendArgCount;
            i++)
        {
            KagamiCmdletParameter? parameter = cmdlet.Parameters[i];

            Type type = parameter.Type;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                type = type.GenericTypeArguments[0];

            if ((parameter.Type == typeof(Bot) || parameter.Type == typeof(GroupMessageEvent)) && TypeParser.Map[parameter.Type](bot, group, string.Empty, out object? obj))
                arguments.Add(obj);
            else if (!TypeParser.Map.TryGetValue(type, out TypeParserDelegate? parser)) // 获取解析器
                throw new NotSupportedException($"类型解析器器不支持的类型 \"{type.FullName}\". ");
            else if (parser(bot, group, args[p], out obj)) // 解析字符串
            {
                arguments.Add(obj);
                p++;
            }
            //else if (parameter.HasDefault) // failback使用默认值
            //    arguments.Add(parameter.Default);
            else // 失败
                return false;

        }

        if (arguments.Count < cmdlet.Parameters.Length)
        {
            for (int i = arguments.Count; i < cmdlet.Parameters.Length; i++)
            {
                KagamiCmdletParameter? parameter = cmdlet.Parameters[i];

                if ((parameter.Type == typeof(Bot) || parameter.Type == typeof(GroupMessageEvent)) && TypeParser.Map[parameter.Type](bot, group, string.Empty, out object? obj))
                {
                    arguments.Add(obj);
                    continue;
                }
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

        KagamiCmdlet[]? cmdset = set.Where(i => matcher(cmd, i.Name, i.IgnoreCase
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal))
            .OrderBy(i => i.Parameters.Length) // 按参数数量从小到大排序
            .ToArray();

        if (cmdset.Length is 0)
            return null; // 找不到匹配的命令

        foreach (KagamiCmdlet? cmdlet in cmdset)
        {
            if (skipPrefix)
                args[0] = args[0][cmdlet.Name.Length..];
            if (ParseArguments(cmdlet, bot, group, args, out object?[]? parameters))
                return await cmdlet.InvokeCommandAsync(bot, group, parameters);
        }

        throw new InvalidOperationException("找不到合适的cmdlet重载");
    }
}
