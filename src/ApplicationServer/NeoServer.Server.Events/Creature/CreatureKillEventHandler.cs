using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Creature;

public class CreatureKillEventHandler(IPlayerSkullService playerSkullService)
    : IEventHandler
{
    public void Execute(ICombatActor killer, ICombatActor victim, bool lastHit, bool justified)
    {
        if (!justified && killer is IPlayer aggressor && victim is IPlayer)
        {
            playerSkullService.UpdatePlayerSkull(aggressor);
        }
    }
}