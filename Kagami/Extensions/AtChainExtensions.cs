namespace Kagami.Extesnions;

public static class BaseChainExtensions
{
    public static ArgTypes.At AsAt(this Konata.Core.Message.Model.AtChain at) => new(at.AtUin);

    public static TChain? As<TChain>(this Konata.Core.Message.BaseChain chain)
        where TChain : Konata.Core.Message.BaseChain => chain as TChain;
}