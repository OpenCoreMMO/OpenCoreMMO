using Moq;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Creatures.Services;
using NeoServer.Domain.Items.Services.ItemTransform;
using NeoServer.Domain.Items.Services.ItemTransform.Operations;
using NeoServer.Domain.Tests.Server;
using NeoServer.Domain.World.Map;
using NeoServer.Domain.World.Services;

namespace NeoServer.Domain.Tests.Helpers.Services;

public static class ItemTransformServiceTestBuilder
{
    public static ItemTransformService Build(IMap map, IItemTypeStore itemTypeStore)
    {
        var staticToDynamicTileServiceMock = new Mock<IStaticToDynamicTileService>();

        var creatureMovementService =
            new CreatureMovementService(map, new CylinderOperation(map), new CreatureMovementValidation(map),
                staticToDynamicTileServiceMock.Object);
        
        var itemFactory = ItemFactoryTestBuilder.Build();
        return new ItemTransformService(itemFactory, map, itemTypeStore, null ,
            new ReplaceGroundOperation(creatureMovementService, map));
    }
}