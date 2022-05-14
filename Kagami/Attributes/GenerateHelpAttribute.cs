namespace Kagami.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class GenerateHelpAttribute : Attribute
{
    public GenerateHelpAttribute(string beforeHelp)
    {

    }
    public string DefaultPrefix { init; get; } = "";
    public string DefaultSuffix { init; get; } = "";
}