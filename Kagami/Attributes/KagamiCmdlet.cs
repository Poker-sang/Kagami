using Konata.Core.Common;

namespace Kagami.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
internal class KagamiCmdletAttribute : Attribute
{
    public KagamiCmdletAttribute(string name)
    {

    }

    /// <summary>
    /// Default <see cref="RoleType.Member"/>
    /// </summary>
    public RoleType Permission { get; init; } = RoleType.Member;

    /// <summary>
    /// Default <see cref="CommandType.Normal"/>
    /// </summary>
    public CommandType CommandType { get; init; } = CommandType.Normal;

    /// <summary>
    /// Default <see langword="true"/>
    /// </summary>
    public bool IgnoreCase { get; init; } = true;
}
