using Kagami.Core;

namespace Kagami.ArgTypes;

/// <summary>
/// 生字符串
/// </summary>
public struct Raw
{
    public Raw(string raw)
    {
        RawString = raw;
        SplitedArgs = raw.SplitRawString();
    }

    public string RawString { get; }

    public string[] SplitedArgs { get; }
}
