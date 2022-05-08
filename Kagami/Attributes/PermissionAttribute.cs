using Konata.Core.Common;
using System;

namespace Kagami.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public class PermissionAttribute : Attribute
{
    public PermissionAttribute(RoleType role)
    {

    }
}