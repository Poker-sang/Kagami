using System;

namespace Kagami.Attributes;

/// <summary>
/// 使用DefaultPrefix（Prefix）+方法名+DefaultSuffix（Suffix）或Name作为调用指令
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class HelpArgsAttribute : Attribute
{
    public HelpArgsAttribute(params Type[] args)
    {

    }
}