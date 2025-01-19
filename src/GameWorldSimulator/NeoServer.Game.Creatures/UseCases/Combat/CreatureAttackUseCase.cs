using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.UseCase.Combat;

namespace NeoServer.Game.Creatures.UseCases.Combat;

public class CreatureAttackUseCase: ICreatureAttackUseCase
{
    public void Execute(ICreature creature, ICreature victim, CombatAttackResult[] attacks)
    {
        throw new System.NotImplementedException();
    }
}