using System.Reflection;

namespace Kagami.UsedTypes;

internal record Record<TAttribute>(
    TAttribute Attribute,
    bool IsObsoleted,
    KagamiParameter[] Parameters,
    string Description,
    MethodInfo Method) where TAttribute : Attribute, IKagamiAttribute;
