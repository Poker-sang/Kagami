using Kagami.ArgTypes;
using Konata.Core.Message;
using Konata.Core.Message.Model;

namespace Kagami.Extensions;

public static class BaseChainExtensions
{
    public static TChain? FetchChain<TChain>(this MessageChain chain) where TChain : BaseChain => (TChain?)chain.FirstOrDefault(i => i is TChain);
    public static IEnumerable<TChain> FetchChains<TChain>(this MessageChain chain) where TChain : BaseChain => chain.Where(i => i is TChain).Cast<TChain>();
    public static At AsAt(this AtChain at) => new(at.AtUin);

    public static Reply AsReply(this ReplyChain reply)
    {
        var map = reply.ToString()[10..^1]
            .Split(',')
            .Select(i => i.Split('='))
            .ToDictionary(i => i[0], i => i[1]);

        return new(
            uint.Parse(map["qq"]),
            uint.Parse(map["seq"]),
            long.Parse(map["uuid"]),
            uint.Parse(map["time"]),
            map["content"]);
    }

    public static Image AsImage(this ImageChain img) => new("http://gchat.qpic.cn" + img.ImageUrl);
}
