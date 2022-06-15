using Kagami.Interfaces;
using System.Reflection;

namespace Kagami.Records;

internal record Record<TAttribute>(
    TAttribute Attribute,
    bool IsObsoleted,
    KagamiParameter[] Parameters,
    string Description,
    MethodInfo Method) where TAttribute : Attribute, IKagamiAttribute;
