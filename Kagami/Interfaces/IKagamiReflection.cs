using Konata.Core.Common;

namespace Kagami.Interfaces;
internal interface IKagamiReflectable
{
    RoleType Permission { get; }
    KagamiParameter[] Parameters { get; }
    string Description { get; }
    object? Target { get; }
    Type ReturnType { get; }
    Func<object?, object?[]?, object?> Method { get; }
}
