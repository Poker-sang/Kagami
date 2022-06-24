using Kagami.ArgTypes;
using Konata.Core;
using Konata.Core.Events.Model;
using Konata.Core.Message;
using Konata.Core.Message.Model;

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
/// <returns>
/// 解析状态<br/>
/// 成功则返回<see langword="object"/><br/>
/// 失败则返回<see langword="null"/><br/>
/// </returns>
public delegate object? TypeParserDelegate(
    in Bot bot,
    in GroupMessageEvent group,
    in string raw);

/// <summary>
/// 类型解析器
/// </summary>
public static class TypeParser
{
    private static readonly Dictionary<Type, int> cache = new();
    public static void Clear() => cache.Clear();

    public static Dictionary<Type, TypeParserDelegate> Map { get; } = new()
    {
        { typeof(string), String },
        { typeof(int),  Int32 },
        { typeof(uint), UInt32 },
        { typeof(PicSource), Enum<PicSource> },
        { typeof(MemeOption), Enum<MemeOption> },
        { typeof(Languages), Enum<Languages> },
        { typeof(NovelDream), Enum<NovelDream> },
        { typeof(ArgTypes.AiModel), Enum<ArgTypes.AiModel> },
        { typeof(At), At },
        { typeof(Reply), Reply },
        { typeof(Image), Image },
        { typeof(string[]), StringArray }
    };

    private static object? String(in Bot bot, in GroupMessageEvent group, in string raw)
        // 过滤<.../>
        => !(string.IsNullOrWhiteSpace(raw) || (raw.StartsWith('<') && raw.EndsWith("/>"))) ? raw : null;

    private static object? Int32(in Bot bot, in GroupMessageEvent group, in string raw)
        => int.TryParse(raw, out var tmp) ? tmp : null;

    private static object? UInt32(in Bot bot, in GroupMessageEvent group, in string raw)
        => uint.TryParse(raw, out var tmp) ? tmp : null;

    private static object? Enum<TEnum>(in Bot bot, in GroupMessageEvent group, in string raw) where TEnum : struct, Enum
        => System.Enum.TryParse(raw, true, out TEnum tmp) && System.Enum.IsDefined(tmp) ? tmp : null;

    private static object? At(in Bot? bot, in GroupMessageEvent group, in string raw) => NextChain<AtChain>(group)?.AsAt();

    private static object? Reply(in Bot bot, in GroupMessageEvent group, in string raw) => NextChain<ReplyChain>(group)?.AsReply();

    private static object? Image(in Bot bot, in GroupMessageEvent group, in string raw) => NextChain<ImageChain>(group)?.AsImage();

    private static object? StringArray(in Bot bot, in GroupMessageEvent group, in string raw)
        => group.Chain.FetchChains<TextChain>().SelectMany(x => x.Content.SplitRawString()).ToArray();

    private static TChain? NextChain<TChain>(in GroupMessageEvent group) where TChain : BaseChain
    {
        var chains = group.Chain.FetchChains<TChain>().ToArray();
        if (!cache.TryGetValue(typeof(TChain), out var index))
            cache[typeof(TChain)] = 0;

        if (index >= chains.Length)
            return null;

        cache[typeof(TChain)] = index + 1;
        return chains[index];
    }
}
