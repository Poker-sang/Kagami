using Kagami.ArgTypes;
using Kagami.Attributes;
using Kagami.Enums;
using Kagami.Records;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message;
using System.ComponentModel;
using System.Reflection;

namespace Kagami.Core;

/// <summary>
/// 处理命令
/// </summary>
public static class CommandParser
{
    /// <summary>
    /// 尝试反射获取命令
    /// </summary>
    /// <param name="type">类型</param>
    /// <param name="method">方法</param>
    /// <param name="attribute"></param>
    /// <returns></returns>
    internal static Record<CmdletAttribute>? Get(MethodInfo method, CmdletAttribute attribute)
    {
        // 方法类型不对的
        if (!(method.ReturnType.IsAssignableFrom(typeof(MessageBuilder))
            || method.ReturnType.IsAssignableFrom(typeof(Task<MessageBuilder>))
            || method.ReturnType.IsAssignableFrom(typeof(ValueTask<MessageBuilder>))))
        {
            Console.Error.WriteLine($"警告: 命令方法\"[{method.ReflectedType?.FullName}]::{method.Name}()\"的返回类型不正确, 将忽略这个命令！");
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
            attribute,
            parameters,
            method.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "",
            method);

        // Console.WriteLine($"发现Cmdlet: {cmdlet.Permission} {cmdlet.CommandType} {cmdlet.ReturnType} {(cmdlet.IgnoreCase ? "" : "*")}{cmdlet.Name}({string.Join(", ", cmdlet.Parameters.Select(i => $"{i.Type} {i.Name}{(i.HasDefault ? $" = {i.Default}" : "")}"))})");
    }

    /// <summary>
    /// 解析命令
    /// </summary>
    /// <param name="raw">原始字符串</param>
    /// <returns></returns>
    [Trigger(TriggerPriority.Cmdlet)]
    public static async Task<bool> Process(Bot bot, GroupMessageEvent group, Raw raw)
    {
        try
        {
            if (raw.SplitedArgs[0].Trim().Contains(' '))
                return false;

            MessageBuilder? result = null;

            if (BotResponse.Cmdlets.TryGetValue(CmdletType.Default, out var set))
                result = await ParseCommand(bot, group, raw, CmdletType.Default, set);

            if (result is null &&
                BotResponse.Cmdlets.TryGetValue(CmdletType.Prefix, out set))
                result = await ParseCommand(bot, group, raw, CmdletType.Prefix, set);

            if (result is not null)
            {
                _ = await bot.SendGroupMessage(group.GroupUin, result);
                return true;
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.ToString());
        }

        return false;
    }

    /// <summary>
    /// 解析命令
    /// </summary>
    /// <param name="bot">机器人实例</param>
    /// <param name="group">群消息事件实例</param>
    /// <param name="raw">生字符串</param>
    /// <param name="type">命令类型</param>
    /// <param name="set">Cmdlet集</param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">命令名不能为空</exception>
    /// <exception cref="InvalidOperationException">找不到合适的命令</exception>
    /// <exception cref="NotSupportedException">类型解析器不支持的类型</exception>
    private static async Task<MessageBuilder?> ParseCommand(
        Bot bot,
        GroupMessageEvent group,
        Raw raw,
        CmdletType type,
        HashSet<Record<CmdletAttribute>> set)
    {
        Func<string?, string, StringComparison, bool> matcher = type switch
        {
            CmdletType.Default => string.Equals,
            CmdletType.Prefix => (i, o, s) => i?.StartsWith(o, s) ?? false,
            _ => throw new ArgumentOutOfRangeException(nameof(type)),
        };

        var cmd = raw.SplitedArgs[0].Trim();
        var cmdset = set.Where(i => matcher(cmd, i.Attribute.Name,
            i.Attribute.IgnoreCase
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal))
            // 按参数数量从大到小排序
            .OrderBy(i => -i.Parameters.Length)
            .ToArray();

        string[] args;

        foreach (var cmdlet in cmdset)
        {
            switch (type)
            {
                case CmdletType.Default:
                    args = raw.SplitedArgs[1..];
                    break;
                case CmdletType.Prefix:
                    args = raw.SplitedArgs;
                    args[0] = args[0][cmdlet.Attribute.Name.Length..];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type));
            }

            if (cmdlet.ParseArguments(bot, group, raw, args, out var parameters))
                return await cmdlet.InvokeAsync<MessageBuilder, CmdletAttribute>(bot, group, parameters);
        }

        // 找不到合适的Cmdlet重载
        return null;
    }
}
