namespace Kagami.Extesnions;

public static class AtChainExtensions
{
    public static ArgTypes.At AsAt(this Konata.Core.Message.Model.AtChain at) => new(at.AtUin);
}