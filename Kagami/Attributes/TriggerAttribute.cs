using System;

namespace Kagami.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class TriggerAttribute : Attribute
{
    public TriggerAttribute(string summary, string trig)
    {

    }
}