using System;

namespace Kagami.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class EnumHelpAttribute : Attribute
{
    public EnumHelpAttribute(string summary)
    {

    }
}