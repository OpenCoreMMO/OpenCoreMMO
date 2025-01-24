using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Creatures.Models.Bases.Events;

namespace NeoServer.Server.Events.Creature;

public class CreatureKillEventHandler(IPlayerSkullService playerSkullService)
    : IApplicationEventHandler<CreatureKillEvent>
{
    public void Handle(CreatureKillEvent @event)
    {
        if (@event.Unjustified && @event.Killer is IPlayer aggressor && @event.Victim is IPlayer)
        {
            playerSkullService.UpdatePlayerSkull(aggressor);
        }
    }
}