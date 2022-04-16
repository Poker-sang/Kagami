using System;

namespace Kagami.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class GenerateHelpAttribute : Attribute
{
    public GenerateHelpAttribute(string beforeHelp)
    {

    }
    public string DefaultPrefix { init; get; }
}