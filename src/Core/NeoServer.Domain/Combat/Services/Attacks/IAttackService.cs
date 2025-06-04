using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.Combat.Services.Attacks;

public interface IAttackService
{
    Result Execute(AttackInput attackInput);
}