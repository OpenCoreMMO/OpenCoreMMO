using Moq;
using NeoServer.Data.InMemory.DataStores;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Factories;
using NeoServer.Domain.Items.Factories;
using NeoServer.Domain.Tests.Helpers.Map;
using NeoServer.Domain.World.Models.Spawns;
using NeoServer.Domain.World.Services;
using Serilog;
using PathFinder = NeoServer.Domain.World.Map.PathFinder;

namespace NeoServer.Domain.Tests.Helpers;

public static class NpcTestDataBuilder
{
    public static INpc Build(string name, INpcType npcType)
    {
        var logger = new Mock<ILogger>();
        var itemFactory = new ItemFactory(null, null, null, null, null, null, null, null, null, null);

        var npcStore = new NpcStore();
        npcStore.AddOrUpdate(name, npcType);

        var coinTypeStore = new CoinTypeStore();

        var map = MapTestDataBuilder.Build(100, 110, 100, 110, 7, 7);
        var pathFinder = new PathFinder(map);
        var mapTool = new MapTool(map, pathFinder);

        var spawnPoint = new SpawnPoint(new Location(105, 105, 7), 60);

        var npcFactory = new NpcFactory(logger.Object, itemFactory, npcStore, coinTypeStore, mapTool);

        var npc = npcFactory.Create(name, spawnPoint);

        npc.SetNewLocation(new Location(105, 105, 7));
        map.PlaceCreature(npc);

        return npc;
    }
}