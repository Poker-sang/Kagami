using Kagami.Enums;
using Kagami.Interfaces;
using Konata.Core.Common;

namespace Kagami.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
internal class CmdletAttribute : Attribute, IKagamiPermission
{
    public CmdletAttribute(string name) => Name = name;

    public string Name { get; init; }

    /// <summary>
    /// Default <see cref="RoleType.Member"/>
    /// </summary>
    public RoleType Permission { get; init; } = RoleType.Member;

    /// <summary>
    /// Default <see cref="CmdletType.Normal"/>
    /// </summary>
    public CmdletType CmdletType { get; init; } = CmdletType.Normal;

    /// <summary>
    /// Default <see langword="true"/>
    /// </summary>
    public bool IgnoreCase { get; init; } = true;
}
