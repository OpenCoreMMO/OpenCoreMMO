using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Game.Common.Contracts.UseCase.Combat;

public interface ICreatureAttackUseCase
{
    void Execute(ICreature creature, ICreature victim, CombatAttackResult[] attacks);
}