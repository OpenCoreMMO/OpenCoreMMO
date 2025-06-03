using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Items.Factories;
using NeoServer.Domain.Items.Factories.AttributeFactory;

namespace NeoServer.Game.Tests.Server;

public static class ItemFactoryTestBuilder
{
    public static IItemFactory Build(params IItemType[] itemTypes)
    {
        var itemTypeStore = ItemTypeStoreTestBuilder.Build(itemTypes);

        return new ItemFactory(null, null, new WeaponFactory(new ChargeableFactory(), itemTypeStore), null, null, null,
            null, null, itemTypeStore, null);
    }

    public static IItemFactory Build(IItemTypeStore itemTypeStore, IMap map = null)
    {
        var chargeableFactory = new ChargeableFactory();

        return new ItemFactory(null, new DefenseEquipmentFactory(itemTypeStore, chargeableFactory),
            new WeaponFactory(new ChargeableFactory(), itemTypeStore), null, null, null, null, null, itemTypeStore,
            null);
    }
}