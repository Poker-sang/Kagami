using Kagami.Attributes;
using Kagami.Enums;
using Kagami.Interfaces;
using Konata.Core;
using Konata.Core.Common;
using Konata.Core.Events.Model;
using System.ComponentModel;
using System.Reflection;

namespace Kagami.Core;

internal record KagamiTrigger(
    TriggerPriority TriggerType,
    RoleType Permission,
    KagamiParameter[] Parameters,
    string Description,
    object? Target,
    Type ReturnType,
    Func<object?, object?[]?, object?> Method) : IKagamiReflectable;

internal static class TriggerParser
{
    internal static KagamiTrigger? GetTrigger(Type type, MethodInfo method, KagamiTriggerAttribute attribute)
    {

        if (!(method.ReturnType.IsAssignableFrom(typeof(bool))
            || method.ReturnType.IsAssignableFrom(typeof(Task<bool>))
            || method.ReturnType.IsAssignableFrom(typeof(ValueTask<bool>))))
        {
            Console.Error.WriteLine($"警告: 触发方法\"[{type.FullName}]::{method.Name}()\"的返回类型不正确, 将忽略这个命令!");
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
            attribute.TriggerPriority,
            attribute.Permission,
            parameters,
            method.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "",
            null!,
            method.ReturnType,
            method.Invoke);
    }

    internal static async Task<bool> ProcessTrigger(
        string raw,
        string[] args,
        List<KagamiTrigger> set,
        Bot bot,
        GroupMessageEvent group)
    {
        if (string.IsNullOrEmpty(raw))
            throw new ArgumentException($"\"{nameof(raw)}\" 不能为 null 或空。", nameof(raw));

        foreach (var trigger in set)
        {
            if (ParserUtilities.ParseArguments(trigger, bot, group, raw, args, out var parameters))
                if (await trigger.InvokeAsync<bool>(bot, group, parameters))
                    return true;
        }

        return false;
    }
}
