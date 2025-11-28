using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Creatures.Services;
using NeoServer.Domain.Items.Services.ItemTransform;
using NeoServer.Domain.Tests.Server;
using NeoServer.Domain.World.Map;
using NeoServer.Domain.World.Services;

namespace NeoServer.Domain.Tests.Helpers.Services;

public static class ItemTransformServiceTestBuilder
{
    public static ItemTransformService Build(IMap map, IItemTypeStore itemTypeStore)
    {
        var creatureMovementService =
            new CreatureMovementService(map, new CylinderOperation(map), new CreatureMovementValidation(map));
        var mapService = new MapService(map, creatureMovementService);
        var itemFactory = ItemFactoryTestBuilder.Build();
        return new ItemTransformService(itemFactory, map, mapService, itemTypeStore, null);
    }
}