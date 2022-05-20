using Kagami.ArgTypes;
using Konata.Core;
using Konata.Core.Events.Model;
using System.Diagnostics.CodeAnalysis;

namespace Kagami.Core;

/// <summary>
/// 类型解析器委托
/// </summary>
/// <remarks>
/// 为什么需要<seealso cref="Bot"/>和<seealso cref="GroupMessageEvent"/>的实例? <br/>
/// 这两个实例主要用于一些需要从该实例中提取属性的对象解析
/// </remarks>
/// <param name="bot">机器人实例</param>
/// <param name="group">群消息事件</param>
/// <param name="raw">字符串</param>
/// <param name="obj">解析后的对象</param>
/// <returns>
/// 解析状态<br/>
/// 成功则返回<see langword="ture"/><br/>
/// 失败则返回<see langword="false"/><br/>
/// </returns>
public delegate bool TypeParserDelegate(
    in Bot bot,
    in GroupMessageEvent group,
    [NotNullWhen(true)] in string raw,
    [NotNullWhen(true)] out object? obj);

/// <summary>
/// 类型解析器
/// </summary>
public static class TypeParser
{
    private static readonly Dictionary<string, object?> Cache = new();
    public static void Clear() => Cache.Clear();

    public static Dictionary<Type, TypeParserDelegate> Map { get; } = new()
    {
        { typeof(string), String },
        { typeof(string[]), StringArray },
        { typeof(int),  Int32 },
        { typeof(uint), UInt32 },
        { typeof(PicSource), Enum<PicSource> },
        { typeof(MemeOption), Enum<MemeOption> },
        { typeof(At), At },
        { typeof(Reply), Reply },
    };

    private static bool String(in Bot bot, in GroupMessageEvent group, in string raw, [NotNullWhen(true)] out object? obj)
        // 过滤<.../>
        => (!(string.IsNullOrWhiteSpace(raw) || raw.StartsWith('<') && raw.EndsWith("/>"))).Set(raw, out obj);

    private static bool Int32(in Bot bot, in GroupMessageEvent group, in string raw, [NotNullWhen(true)] out object? obj)
        => int.TryParse(raw, out var tmp).Set(tmp, out obj);

    private static bool UInt32(in Bot bot, in GroupMessageEvent group, in string raw, [NotNullWhen(true)] out object? obj)
        => uint.TryParse(raw, out var tmp).Set(tmp, out obj);

    private static bool Enum<TEnum>(in Bot bot, in GroupMessageEvent group, in string raw, [NotNullWhen(true)] out object? obj) where TEnum : struct, Enum
        => System.Enum.TryParse(raw, true, out TEnum tmp).Set(tmp, out obj);

    private static bool At(in Bot? bot, in GroupMessageEvent group, in string raw, [NotNullWhen(true)] out object? obj)
    {
        var result = Chain<Konata.Core.Message.Model.AtChain>(group, out var tmp);
        obj = ((Konata.Core.Message.Model.AtChain?)tmp)?.AsAt();
        return result;
    }
    private static bool Reply(in Bot bot, in GroupMessageEvent group, in string raw, [NotNullWhen(true)] out object? obj)
    {
        var result = Chain<Konata.Core.Message.Model.ReplyChain>(group, out var tmp);
        obj = ((Konata.Core.Message.Model.ReplyChain?)tmp)?.AsReply();
        return result;
    }
    private static bool StringArray(in Bot bot, in GroupMessageEvent group, in string raw, [NotNullWhen(true)] out object? obj)
    {
        obj = group?.Chain.Where(x => x.Type is Konata.Core.Message.BaseChain.ChainType.Text).SelectMany(x => ParserUtilities.SplitRawString(x.As<Konata.Core.Message.Model.TextChain>()!.Content)).ToArray();
        return obj is string[];
    }

    private static bool Chain<TChain>(in GroupMessageEvent group, [NotNullWhen(true)] out object? obj)
        where TChain : Konata.Core.Message.BaseChain
    {
        if (group is null)
        {
            obj = null;
            return false;
        }

        var cacheName = typeof(TChain).Name;
        var cacheIndexName = "p" + typeof(TChain).Name;

        TChain[] chains;
        int index;
        if (Cache.TryGetValue(cacheName, out var oAts))
        {
            _ = Cache.TryGetValue(cacheIndexName, out var oIndex);
            index = (int)(oIndex ?? 0);
            chains = (TChain[])(oAts ?? throw new InvalidProgramException("程序内部逻辑错误!"));
        }
        else
        {
            Cache[cacheIndexName] = index = 0;
            Cache[cacheName] = chains = group
                .Chain
                .Where(i => i is TChain)
                .Select(i => i.As<TChain>()!)
                .ToArray();
        }

        if (chains is { Length: 0 })
        {
            obj = null;
            return false;
        }

        obj = chains[index];
        Cache[cacheIndexName] = index + 1;
        return true;
    }
    //private static bool MessageChain(in Bot? bot, in GroupMessageEvent? group, in string raw, [NotNullWhen(true)] out object? obj)
    //{
    //    obj = group?.Chain;
    //    return obj is Konata.Core.Message.MessageChain;
    //}

    private static bool Set(this bool b, in object i, out object o)
    {
        o = i;
        return b;
    }
}