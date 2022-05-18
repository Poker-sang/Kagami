using System.Diagnostics.CodeAnalysis;

using Konata.Core;
using Konata.Core.Events.Model;

namespace Kagami;

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
    in Bot? bot,
    in GroupMessageEvent? group,
    [NotNullWhen(true)] in string raw,
    [NotNullWhen(true)] out object? obj);

/// <summary>
/// 类型解析器
/// </summary>
public static class TypeParser
{
    private static readonly Dictionary<string, object?> s_cache = new();
    public static void Clear() => s_cache.Clear();

    public static Dictionary<Type, TypeParserDelegate> Map { get; } = new()
    {
        { typeof(Bot), Bot },
        { typeof(string), String },
        { typeof(int),  Int32 },
        { typeof(uint), UInt32 },
        { typeof(ArgTypes.PicSource), Enum<ArgTypes.PicSource> },
        { typeof(ArgTypes.MemeOption), Enum<ArgTypes.MemeOption> },
        { typeof(ArgTypes.At), At },
        { typeof(ArgTypes.Reply), Reply },
        { typeof(GroupMessageEvent), GroupMessageEvent },
        { typeof(string[]), StringArray },
    };

    private static bool Bot(in Bot? bot, in GroupMessageEvent? group, in string raw, [NotNullWhen(true)] out object? obj)
        => (bot is not null).Set(bot!, out obj);
    private static bool String(in Bot? bot, in GroupMessageEvent? group, in string raw, [NotNullWhen(true)] out object? obj)
        => !string.IsNullOrWhiteSpace(raw).Set(raw, out obj);
    private static bool Int32(in Bot? bot, in GroupMessageEvent? group, in string raw, [NotNullWhen(true)] out object? obj)
        => int.TryParse(raw, out int tmp).Set(tmp, out obj);
    private static bool UInt32(in Bot? bot, in GroupMessageEvent? group, in string raw, [NotNullWhen(true)] out object? obj)
        => uint.TryParse(raw, out uint tmp).Set(tmp, out obj);
    private static bool Enum<TEnum>(in Bot? bot, in GroupMessageEvent? group, in string raw, [NotNullWhen(true)] out object? obj)
        where TEnum : struct
        => System.Enum.TryParse(raw, true, out TEnum tmp).Set(tmp, out obj);
    private static bool At(in Bot? bot, in GroupMessageEvent? group, in string raw, [NotNullWhen(true)] out object? obj)
    {
        bool result = Chain<Konata.Core.Message.Model.AtChain>(group, out object? tmp);
        obj = ((Konata.Core.Message.Model.AtChain)tmp!).AsAt();
        return result;
    }
    private static bool Reply(in Bot? bot, in GroupMessageEvent? group, in string raw, [NotNullWhen(true)] out object? obj)
    {
        bool result = Chain<Konata.Core.Message.Model.ReplyChain>(group, out object? tmp);
        obj = ((Konata.Core.Message.Model.ReplyChain)tmp!).AsReply();
        return result;
    }
    private static bool Chain<TChain>(in GroupMessageEvent? group, [NotNullWhen(true)] out object? obj)
        where TChain : Konata.Core.Message.BaseChain
    {
        if (group is null)
        {
            obj = null;
            return false;
        }
        string cacheName = typeof(TChain).Name;
        string cacheIndexName = "p" + typeof(TChain).Name;

        TChain[] chains;
        int index;
        if (s_cache.TryGetValue(cacheName, out object? oAts))
        {
            s_cache.TryGetValue(cacheIndexName, out object? oIndex);
            index = (int)(oIndex ?? 0);
            chains = (TChain[])(oAts ?? throw new InvalidProgramException("程序内部逻辑错误!"));
        }
        else
        {
            s_cache[cacheIndexName] = index = 0;
            s_cache[cacheName] = chains = group
                .Chain
                .Where(i => i is TChain)
                .Select(i => i.As<TChain>()!)
                .ToArray();
            if (chains?.Length is 0 or null)
            {
                obj = null;
                return false;
            }
        }
        obj = chains[index];
        s_cache[cacheIndexName] = index + 1;
        return true;
    }
    //private static bool MessageChain(in Bot? bot, in GroupMessageEvent? group, in string raw, [NotNullWhen(true)] out object? obj)
    //{
    //    obj = group?.Chain;
    //    return obj is Konata.Core.Message.MessageChain;
    //}
    private static bool StringArray(in Bot? bot, in GroupMessageEvent? group, in string raw, [NotNullWhen(true)] out object? obj)
    {
        obj = group?.Chain.Where(x => x.Type is Konata.Core.Message.BaseChain.ChainType.Text).SelectMany(x => Entry.SplitCommand(x.As<Konata.Core.Message.Model.TextChain>()!.Content)).ToArray();
        return obj is string[];
    }
    private static bool GroupMessageEvent(in Bot? bot, in GroupMessageEvent? group, in string raw, [NotNullWhen(true)] out object? obj)
        => (group is not null).Set(group!, out obj);


    private static bool Set(this bool b, in object i, out object o)
    {
        o = i;
        return b;
    }
}
