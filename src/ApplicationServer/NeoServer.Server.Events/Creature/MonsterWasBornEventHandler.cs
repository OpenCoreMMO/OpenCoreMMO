using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Creatures.Events.Monster;
using NeoServer.Domain.Creatures.Monster.Services;

namespace NeoServer.Server.Events.Creature;

public class MonsterWasBornEventHandler(IMap map, ICreatureGameInstance creatureGameInstance, MonsterStateService monsterStateService): IApplicationEventHandler<MonsterWasBornEvent>
{
    public void Handle(MonsterWasBornEvent @event)
    {
        creatureGameInstance.Add(@event.Monster);
        map.PlaceCreature(@event.Monster);
        monsterStateService.UpdateState(@event.Monster);
    }
}