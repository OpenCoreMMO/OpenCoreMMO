﻿using Moq;
using NeoServer.Application.Features.Item.Decay;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Creature.Factories;
using NeoServer.Game.Item.Factories;
using NeoServer.Game.Tests.Helpers.Map;
using NeoServer.Game.Tests.Server;
using NeoServer.Game.World.Models.Spawns;
using NeoServer.Game.World.Services;
using NeoServer.Infrastructure.InMemory;
using Serilog;
using PathFinder = NeoServer.Game.World.Map.PathFinder;

namespace NeoServer.Game.Tests.Helpers;

public static class NpcTestDataBuilder
{
    public static INpc Build(string name, INpcType npcType)
    {
        var logger = new Mock<ILogger>();
        var itemFactory = new ItemFactory(null, null, null, null, null, null,
            null, null, null, 
            new IItemEventSubscriber[]
            {
                new DecayItemSubscriber(ItemDecayServiceTestBuilder.BuildTracker())
            });

        var npcStore = new NpcStore();
        npcStore.Add(name, npcType);

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