using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Services;

namespace NeoServer.Server.Events.Combat;

public class CreatureAttackEventHandler(IPlayerSkullService playerSkullService)
{
    public void Execute(ICreature creature, ICreature victim, CombatAttackResult[] attacks)
    {
        if (creature is IPlayer aggressor && victim is IPlayer victimPlayer)
        {
            playerSkullService.UpdateSkullOnAttack(aggressor, victimPlayer);
        }
    }
}