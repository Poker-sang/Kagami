using Kagami.Enums;
using Kagami.Interfaces;
using Konata.Core.Common;

namespace Kagami.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
internal class TriggerAttribute : Attribute, IKagamiAttribute
{
    public TriggerAttribute(TriggerPriority triggerPriority) => TriggerPriority = triggerPriority;

    public TriggerPriority TriggerPriority { get; set; }

    /// <summary>
    /// Default <see cref="Kagami.Enums.ParameterType.Default"/>
    /// </summary>
    public ParameterType ParameterType { get; init; } = ParameterType.Default;

    /// <summary>
    /// Default <see cref="RoleType.Member"/>
    /// </summary>
    public RoleType Permission { get; init; } = RoleType.Member;
    
    /// <summary>
    /// Default <see langword="false"/>
    /// </summary>
    public bool IsObsoleted { get; } = false;
}
