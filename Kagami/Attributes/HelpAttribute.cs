using System;

namespace Kagami.Attributes;

/// <summary>
/// 使用DefaultPrefix（Prefix）+方法名+DefaultSuffix（Suffix）或Name作为调用指令
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class HelpAttribute : Attribute
{
    public HelpAttribute(string summary)
    {

    }

    /// <summary>
    /// 覆盖DefaultPrefix
    /// </summary>
    public string Prefix { init; get; }

    /// <summary>
    /// 覆盖DefaultSuffix
    /// </summary>
    public string Suffix { init; get; }

    /// <summary>
    /// 自定义指令名（不使用Prefix和Suffix）
    /// </summary>
    public string Name { init; get; }
}