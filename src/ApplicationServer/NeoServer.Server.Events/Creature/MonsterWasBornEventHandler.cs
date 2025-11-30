using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Creatures.Events.Monster;
using NeoServer.Domain.Creatures.Monster.Services;

namespace NeoServer.Server.Events.Creature;

public class MonsterWasBornEventHandler(
    IMap map,
    MonsterStateService monsterStateService) : IApplicationEventHandler<MonsterWasBornEvent>
{
    public void Handle(MonsterWasBornEvent @event)
    {
        map.PlaceCreature(@event.Monster);
        monsterStateService.UpdateState(@event.Monster);
        //monster will not be added to the creature list here. It will be added when it is awakened.
    }
}