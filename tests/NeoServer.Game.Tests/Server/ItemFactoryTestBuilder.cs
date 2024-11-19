using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Item.Factories;
using NeoServer.Game.Item.Factories.AttributeFactory;
using NeoServer.Modules.ItemManagement.DecayManagement;

namespace NeoServer.Game.Tests.Server;

public static class ItemFactoryTestBuilder
{
    public static IItemFactory Build(ItemDecayTracker itemDecayTracker = null, params IItemType[] itemTypes)
    {
        var itemTypeStore = ItemTypeStoreTestBuilder.Build(itemTypes);

        return new ItemFactory(null, new WeaponFactory(new ChargeableFactory(), itemTypeStore),
            null, null, null, null, null, itemTypeStore, null);
    }

    public static IItemFactory Build(IItemTypeStore itemTypeStore, ItemDecayTracker itemDecayTracker = null)
    {
        var chargeableFactory = new ChargeableFactory();

        return new ItemFactory(new DefenseEquipmentFactory(itemTypeStore, chargeableFactory),
            new WeaponFactory(chargeableFactory, itemTypeStore),
            null, null, null, null, null, itemTypeStore, null);
    }
}