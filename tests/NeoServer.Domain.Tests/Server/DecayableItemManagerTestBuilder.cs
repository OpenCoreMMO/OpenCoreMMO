using Moq;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Creatures.Services;
using NeoServer.Domain.Items.Services;
using NeoServer.Domain.Items.Services.ItemTransform;
using NeoServer.Domain.World.Factories;
using NeoServer.Domain.World.Map;
using NeoServer.Domain.World.Services;
using NeoServer.Server.Managers;
using Serilog;

namespace NeoServer.Domain.Tests.Server;

public class DecayableItemManagerTestBuilder
{
    public static DecayableItemManager Build(IMap map, IItemTypeStore itemTypeStore)
    {
        var creatureMovementService = new CreatureMovementService(map, new CylinderOperation(map));

        var mapService = new MapService(map, creatureMovementService);
        var itemFactory = ItemFactoryTestBuilder.Build();
        var looger = new Mock<ILogger>();
        var tileFactory = new TileFactory(looger.Object);
        var itemTransformService = new ItemTransformService(itemFactory, map, mapService, itemTypeStore, null);
        var decayService = new DecayService(itemTransformService);
        return new DecayableItemManager(decayService);
    }
}