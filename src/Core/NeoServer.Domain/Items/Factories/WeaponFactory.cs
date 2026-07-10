using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Factories.AttributeFactory;
using NeoServer.Domain.Items.Items.Weapons;

namespace NeoServer.Domain.Items.Factories;

public class WeaponFactory : IFactory
{
    private readonly ChargeCounterFactory _chargeCounterFactory;
    private readonly IItemTypeStore _itemTypeStore;

    public WeaponFactory(ChargeCounterFactory chargeCounterFactory, IItemTypeStore itemTypeStore)
    {
        _chargeCounterFactory = chargeCounterFactory;
        _itemTypeStore = itemTypeStore;
    }

    public IItem Create(
        IItemType itemType,
        Location location,
        IDictionary<ItemAttribute, IConvertible> itemAttributes,
        ushort? overrideCharges = null)
    {
        if (MeleeWeapon.IsApplicable(itemType))
        {
            var chargeCounter = _chargeCounterFactory.Create(itemType, overrideCharges);
            return new MeleeWeapon(itemType, location, itemAttributes)
            {
                Charges = chargeCounter,
                ItemTypeFinder = _itemTypeStore.Get
            };
        }

        if (DistanceWeapon.IsApplicable(itemType))
        {
            var chargeCounter = _chargeCounterFactory.Create(itemType, overrideCharges);
            return new DistanceWeapon(itemType, location)
            {
                ItemTypeFinder = _itemTypeStore.Get,
                Charges = chargeCounter
            };
        }

        if (MagicWeapon.IsApplicable(itemType))
        {
            var chargeCounter = _chargeCounterFactory.Create(itemType, overrideCharges);
            return new MagicWeapon(itemType, location)
            {
                ItemTypeFinder = _itemTypeStore.Get,
                Charges = chargeCounter
            };
        }

        if (ICumulative.IsApplicable(itemType))
        {
            if (ThrowableWeapon.IsApplicable(itemType)) return new ThrowableWeapon(itemType, location, itemAttributes);
            if (Ammo.IsApplicable(itemType)) return new Ammo(itemType, location, itemAttributes);
        }

        return null;
    }
}