using Kagami.Enums;
using Konata.Core.Common;

namespace Kagami.Interfaces;

internal interface IKagamiAttribute
{
    ParameterType ParameterType { get; }

    RoleType Permission { get; }
}
