using Konata.Core.Common;

namespace Kagami.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class PermissionAttribute : Attribute
{
    public PermissionAttribute(RoleType role)
    {

    }
}