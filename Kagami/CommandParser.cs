using Kagami.ArgTypes;
using Kagami.Attributes;
using Kagami.Enums;
using Kagami.Interfaces;
using Konata.Core;
using Konata.Core.Common;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace Kagami;

internal record KagamiParameter(Type Type, string Name, bool HasDefault, object? Default, string Description);
internal record KagamiCmdlet(
    string Name,
    RoleType Permission,
    CmdletType CommandType,
    bool IgnoreCase,
    KagamiParameter[] Parameters,
    string Description,
    object? Target,
    Type ReturnType,
    Func<object?, object?[]?, object?> Method) : IKagamiReflectable;

/// <summary>
/// 处理命令
/// </summary>
public static class CommandParser
{
    internal static KagamiCmdlet? GetCommand(Type type, MethodInfo method, KagamiCmdletAttribute attribute)
    {
        // 方法类型不对的
        if (!(method.ReturnType.IsAssignableFrom(typeof(MessageBuilder))
            || method.ReturnType.IsAssignableFrom(typeof(Task<MessageBuilder>))
            || method.ReturnType.IsAssignableFrom(typeof(ValueTask<MessageBuilder>))))
        {
            Console.Error.WriteLine($"警告: 命令方法\"[{type.FullName}]::{method.Name}()\"的返回类型不正确, 将忽略这个命令!");
            return null;
        }

        var parameters = method
                .GetParameters()
                .Select(parameter => new KagamiParameter(
                    parameter.ParameterType,
                    parameter.Name!,
                    parameter.HasDefaultValue,
                    parameter.DefaultValue,
                    parameter.GetCustomAttribute<DescriptionAttribute>()?.Description ?? ""))
                .ToArray();

        return new(
            attribute.Name,
            attribute.Permission,
            attribute.CmdletType,
            attribute.IgnoreCase,
            parameters,
            method.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "",
            null!,
            method.ReturnType,
            method.Invoke);

        // Console.WriteLine($"发现Cmdlet: {cmdlet.Permission} {cmdlet.CommandType} {cmdlet.ReturnType} {(cmdlet.IgnoreCase ? "" : "*")}{cmdlet.Name}({string.Join(", ", cmdlet.Parameters.Select(i => $"{i.Type} {i.Name}{(i.HasDefault ? $" = {i.Default}" : "")}"))})");
    }

    /// <summary>
    /// 解析命令
    /// </summary>
    /// <param name="raw">原始字符串</param>
    /// <returns></returns>
    [KagamiTrigger(TriggerPriority.Cmdlet)]
    public static async Task<bool> ParseRawCommand(Bot bot, GroupMessageEvent group, Raw raw)
    {
        MessageBuilder? result = null;
        try
        {
            var args = raw.RawString.SplitRawString();
            var cmd = args[0].Trim(); // 获取第一个元素用作命令
            if (cmd.Contains(' '))
                throw new InvalidOperationException("命令名中不能包括空格");

            if (BotResponse.Cmdlets.TryGetValue(CmdletType.Normal, out var set))
                result = await ParseCommand(cmd, args[1..], set, bot, group, string.Equals);

            if (result is null && BotResponse.Cmdlets.TryGetValue(CmdletType.Prefix, out set))
                result = await ParseCommand(cmd, args, set, bot, group, (i, o, s) => i?.StartsWith(o, s) ?? false, true);

            if (result is null)
                throw new NotSupportedException("不支持的Cmdlet类型");
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
        }

        if (result is not null)
        {
            _ = await bot.SendGroupMessage(group.GroupUin, result);
            return true;
        }

        return false;
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

            if (ParserUtilities.ParseArguments(cmdlet, bot, group, "", args, out var parameters))
                return await cmdlet.InvokeAsync<MessageBuilder>(bot, group, parameters);
        }

        throw new InvalidOperationException("找不到合适的cmdlet重载");
    }
}
