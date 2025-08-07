using NeoServer.Domain.Common.Combat.Structs;

namespace NeoServer.Domain.Combat.Attacks;

public interface IAttackService
{
    CombatResult Execute(AttackInput attackInput);
}