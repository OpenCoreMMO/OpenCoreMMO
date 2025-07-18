using Moq;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Items.Services.ItemTransform;
using NeoServer.Domain.Tests.Server;
using NeoServer.Domain.World.Factories;
using NeoServer.Domain.World.Services;
using Serilog;

namespace NeoServer.Domain.Tests.Helpers.Services;

public static class ItemTransformServiceTestBuilder
{
    public static ItemTransformService Build(IMap map, IItemTypeStore itemTypeStore)
    {
        var mapService = new MapService(map);
        var itemFactory = ItemFactoryTestBuilder.Build();
        var looger = new Mock<ILogger>();
        var tileFactory = new TileFactory(looger.Object);
        return new ItemTransformService(itemFactory, map, mapService, itemTypeStore, null);
    }
}