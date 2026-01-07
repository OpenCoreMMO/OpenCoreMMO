using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Creatures.Events;

namespace NeoServer.Server.Events.Creature;

public class CreatureChangedVisibilityEventHandler(IMap map)
    : IApplicationEventHandler<CreatureChangedVisibilityEvent>
{
    public void Handle(CreatureChangedVisibilityEvent @event)
    {
        var creature = @event.Creature;
        foreach (var spectator in map.GetSpectators(creature.Location))
            spectator.OnSpectatorChangedVisibility(creature);
    }
}