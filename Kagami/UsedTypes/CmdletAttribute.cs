using Konata.Core.Common;

namespace Kagami.UsedTypes;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
internal class CmdletAttribute : Attribute, IKagamiAttribute
{
    public CmdletAttribute(string name) => Name = name;

    public string Name { get; init; }

    /// <summary>
    /// Default <see cref="RoleType.Member"/>
    /// </summary>
    public RoleType Permission { get; init; } = RoleType.Member;

    /// <summary>
    /// Default <see cref="CmdletType.Default"/>
    /// </summary>
    public CmdletType CmdletType { get; init; } = CmdletType.Default;

    /// <summary>
    /// Default <see cref="ParameterType.Default"/>
    /// </summary>
    public ParameterType ParameterType { get; init; } = ParameterType.Default;

    /// <summary>
    /// Default <see langword="true"/>
    /// </summary>
    public bool IgnoreCase { get; init; } = true;
}
