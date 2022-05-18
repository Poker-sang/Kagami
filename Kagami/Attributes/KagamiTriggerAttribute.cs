using Kagami.Enums;
using Konata.Core.Common;

namespace Kagami.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
internal class KagamiTriggerAttribute : Attribute
{
    public KagamiTriggerAttribute(TriggerPriority triggerPriority) => TriggerPriority = triggerPriority;

    public TriggerPriority TriggerPriority { get; set; }

    /// <summary>
    /// Default <see cref="RoleType.Member"/>
    /// </summary>
    public RoleType Permission { get; init; } = RoleType.Member;
}
