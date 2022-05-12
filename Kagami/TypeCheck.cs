using System.Diagnostics.CodeAnalysis;
using Konata.Core.Events.Model;

namespace Kagami;

/// <summary>
/// 类型检查器
/// </summary>
public static class TypeCheck
{
    public delegate bool TypeCheckDelegate(
        [NotNullWhen(true)] in string raw,
        [NotNullWhen(true)] out object? obj,
        in GroupMessageEvent? group);
    public static Dictionary<Type, TypeCheckDelegate> Map { get; } = new()
    {
        { typeof(string), String},
        { typeof(int),  Int32 },
        { typeof(uint), UInt32 },
        { typeof(ArgTypes.PicCommands), Enum<ArgTypes.PicCommands> },
        { typeof(ArgTypes.At), At },
        { typeof(Konata.Core.Message.MessageChain), MessageChain },
    };
    private static bool String([NotNullWhen(true)] in string raw!!, [NotNullWhen(true)] out object? obj, in GroupMessageEvent? group = null)
        => !string.IsNullOrWhiteSpace((obj = raw) as string);
    private static bool Int32([NotNullWhen(true)] in string raw!!, [NotNullWhen(true)] out object? obj, in GroupMessageEvent? group = null)
        => int.TryParse(raw, out var tmp).Set(tmp, out obj);
    private static bool UInt32([NotNullWhen(true)] in string raw!!, [NotNullWhen(true)] out object? obj, in GroupMessageEvent? group = null)
        => uint.TryParse(raw, out var tmp).Set(tmp, out obj);
    private static bool Enum<TEnum>([NotNullWhen(true)] in string raw!!, [NotNullWhen(true)] out object? obj, in GroupMessageEvent? group = null)
        where TEnum : struct
        => System.Enum.TryParse<TEnum>(raw, out var tmp).Set(tmp, out obj);

    private static bool At([NotNullWhen(true)] in string raw!!, [NotNullWhen(true)] out object? obj, in GroupMessageEvent? group)
    {
        obj = group
            ?.Chain
            .FirstOrDefault(x => x.Type is Konata.Core.Message.BaseChain.ChainType.At)
            ?.As<Konata.Core.Message.Model.AtChain>()
            ?.AsAt();
        return obj is ArgTypes.At;
    }

    private static bool MessageChain([NotNullWhen(true)] in string raw!!, [NotNullWhen(true)] out object? obj, in GroupMessageEvent? group)
    {
        obj = group?.Chain;
        return obj is Konata.Core.Message.MessageChain;
    }

    private static bool Set(this bool b, in object i!!, out object o)
    {
        o = i;
        return b;
    }
}
