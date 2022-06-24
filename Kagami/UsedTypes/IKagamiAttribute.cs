using Konata.Core.Common;

namespace Kagami.UsedTypes;

internal interface IKagamiAttribute
{
    ParameterType ParameterType { get; }

    RoleType Permission { get; }
}
