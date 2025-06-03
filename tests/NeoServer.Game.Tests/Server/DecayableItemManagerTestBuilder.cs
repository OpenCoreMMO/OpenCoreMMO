using Moq;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Items.Services;
using NeoServer.Game.Items.Services.ItemTransform;
using NeoServer.Game.World.Factories;
using NeoServer.Game.World.Services;
using NeoServer.Server.Managers;
using Serilog;

namespace NeoServer.Game.Tests.Server;

public class DecayableItemManagerTestBuilder
{
    public static DecayableItemManager Build(IMap map, IItemTypeStore itemTypeStore)
    {
        var mapService = new MapService(map);
        var itemFactory = ItemFactoryTestBuilder.Build();
        var looger = new Mock<ILogger>();
        var tileFactory = new TileFactory(looger.Object);
        var itemTransformService = new ItemTransformService(itemFactory, map, mapService, itemTypeStore, null);
        var decayService = new DecayService(itemTransformService);
        return new DecayableItemManager(decayService);
    }
}