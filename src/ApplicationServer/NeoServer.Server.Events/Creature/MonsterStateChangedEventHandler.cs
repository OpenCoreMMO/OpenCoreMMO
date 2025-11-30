using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Events.Monster;
using NeoServer.Domain.Creatures.Monster;

namespace NeoServer.Server.Events.Creature;

public class MonsterStateChangedEventHandler(ICreatureGameInstance creatureGameInstance): IApplicationEventHandler<MonsterStateChangedEvent>
{
    public void Handle(MonsterStateChangedEvent @event)
    {
        if (@event.ToState == MonsterState.Sleeping)
        {
            creatureGameInstance.TryRemove(@event.Monster.CreatureId);
        }
        
        if (@event.FromState == MonsterState.Sleeping)
        {
            creatureGameInstance.Add(@event.Monster);
        }
    }
}