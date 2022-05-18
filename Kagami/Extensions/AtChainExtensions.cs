namespace Kagami.Extensions;

public static class BaseChainExtensions
{
    public static ArgTypes.At AsAt(this Konata.Core.Message.Model.AtChain at) => new(at.AtUin);

    public static TChain? As<TChain>(this Konata.Core.Message.BaseChain chain)
        where TChain : Konata.Core.Message.BaseChain => chain as TChain;


    public static ArgTypes.Reply AsReply(this Konata.Core.Message.Model.ReplyChain reply)
    {
        string tmp = reply.ToString();
        var map = tmp.Substring(10, tmp.Length - 11)
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
}
