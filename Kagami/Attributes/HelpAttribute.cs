using System;

namespace Kagami.Attributes;

/// <summary>
/// 默认使用DefaultPrefix（Prefix）+方法名作为调用指令
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
    /// 自定义指令名
    /// </summary>
    public string Name { init; get; }
}