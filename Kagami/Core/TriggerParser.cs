using Kagami.ArgTypes;
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
    internal static KagamiTrigger? Get(MethodInfo method, KagamiTriggerAttribute attribute)
    {
        if (!(method.ReturnType.IsAssignableFrom(typeof(bool))
            || method.ReturnType.IsAssignableFrom(typeof(Task<bool>))
            || method.ReturnType.IsAssignableFrom(typeof(ValueTask<bool>))))
        {
            Console.Error.WriteLine($"警告: 触发方法\"[{method.ReflectedType?.FullName}]::{method.Name}()\"的返回类型不正确, 将忽略这个触发！");
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

    internal static async Task<bool> Process(Bot bot, GroupMessageEvent group, Raw raw)
    {
        var args = raw.RawString.SplitRawString();

        foreach (var trigger in BotResponse.Triggers)
        {
            if (ParserUtilities.ParseArguments(trigger, bot, group, raw, args, out var parameters))
                if (await trigger.InvokeAsync<bool>(bot, group, parameters))
                    return true;
        }

        return false;
    }
}
