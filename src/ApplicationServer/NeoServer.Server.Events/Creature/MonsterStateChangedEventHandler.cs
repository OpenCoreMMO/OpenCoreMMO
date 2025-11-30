using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Events.Monster;
using NeoServer.Domain.Creatures.Monster;

namespace NeoServer.Server.Events.Creature;

public class MonsterStateChangedEventHandler(ICreatureGameInstance creatureGameInstance): IApplicationEventHandler<MonsterStateChangedEvent>
{
    public void Handle(MonsterStateChangedEvent @event)
    {
        // Skip if state hasn't actually changed
        if (@event.FromState == @event.ToState)
            return;
        
        // Monster waking up from sleep - add to game instance
        if (@event.FromState == MonsterState.Sleeping)
        {
            creatureGameInstance.Add(@event.Monster);
            return;
        }
        
        // Monster going to sleep - remove from game instance
        if (@event.ToState == MonsterState.Sleeping)
        {
            creatureGameInstance.TryRemove(@event.Monster.CreatureId);
        }
    }
}