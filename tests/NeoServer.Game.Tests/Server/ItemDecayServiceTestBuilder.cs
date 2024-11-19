using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Item.Services.ItemTransform;
using NeoServer.Game.World.Services;
using NeoServer.Modules.ItemManagement.DecayManagement;

namespace NeoServer.Game.Tests.Server;

public class ItemDecayServiceTestBuilder
{
    public static ItemDecayTracker BuildTracker()
    {
        return new ItemDecayTracker();
    }

    public static ItemDecayProcessor BuildProcessor(IMap map, IItemTypeStore itemTypeStore,
        ItemDecayTracker itemDecayTracker = null)
    {
        var mapService = new MapService(map);
        var itemFactory = ItemFactoryTestBuilder.Build(itemTypeStore, itemDecayTracker);
        var itemTransformService = new ItemTransformService(itemFactory, map, mapService, itemTypeStore);
        return new ItemDecayProcessor(itemTransformService);
    }
}