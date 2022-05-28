using Kagami.ArgTypes;
using Kagami.Attributes;
using Kagami.Records;
using Konata.Core;
using Konata.Core.Events.Model;
using System.ComponentModel;
using System.Reflection;

namespace Kagami.Core;

internal static class TriggerParser
{
    internal static Record<TriggerAttribute>? Get(MethodInfo method, TriggerAttribute attribute)
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
            attribute,
            parameters,
            method.GetCustomAttribute<DescriptionAttribute>()?.Description ?? "",
            method);
    }

    internal static async Task<bool> Process(Bot bot, GroupMessageEvent group, Raw raw)
    {
        foreach (var trigger in BotResponse.Triggers)
        {
            if (trigger.ParseArguments(bot, group, raw, raw.SplitArgs, out var parameters)
                && await trigger.InvokeAsync<bool, TriggerAttribute>(bot, group, parameters))
                return true;
        }

        return false;
    }
}
