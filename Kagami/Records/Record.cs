using Kagami.Interfaces;
using System.Reflection;

namespace Kagami.Records;

internal record Record<TAttribute>(
    TAttribute Attribute,
    KagamiParameter[] Parameters,
    string Description,
    MethodInfo Method) where TAttribute : Attribute, IKagamiPermission;
