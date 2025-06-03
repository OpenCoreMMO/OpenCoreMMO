using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Location.Structs;

namespace NeoServer.Server.Events.Creature;

public class CreatureWasBornEventHandler(IMap map, ICreatureGameInstance creatureGameInstance)
{
    public void Execute(IMonster monster, Location location)
    {
        creatureGameInstance.Add(monster);
        map.PlaceCreature(monster);
    }
}