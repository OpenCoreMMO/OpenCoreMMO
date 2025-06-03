using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Creatures.Models.Bases.Events;

namespace NeoServer.Server.Events.Creature;

public class CreatureKillEventHandler(IPlayerSkullService playerSkullService)
    : IApplicationEventHandler<CreatureKillEvent>
{
    public void Handle(CreatureKillEvent @event)
    {
        if (@event.Unjustified && @event.Killer is IPlayer aggressor && @event.Victim is IPlayer)
            playerSkullService.UpdatePlayerSkull(aggressor);
    }
}