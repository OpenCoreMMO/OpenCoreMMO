using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Items.Factories.AttributeFactory;
using NeoServer.Game.Items.Items.Weapons;

namespace NeoServer.Game.Items.Factories;

public class WeaponFactory : IFactory
{
    private readonly ChargeableFactory _chargeableFactory;
    private readonly IItemTypeStore _itemTypeStore;

    public WeaponFactory(ChargeableFactory chargeableFactory, IItemTypeStore itemTypeStore)
    {
        _chargeableFactory = chargeableFactory;
        _itemTypeStore = itemTypeStore;
    }

    public event CreateItem OnItemCreated;

    public IItem Create(IItemType itemType, Location location, IDictionary<ItemAttribute, IConvertible> attributes)
    {
        var chargeable = _chargeableFactory.Create(itemType);

        if (MeleeWeapon.IsApplicable(itemType))
            return new MeleeWeapon(itemType, location)
            {
                Chargeable = chargeable,
                ItemTypeFinder = _itemTypeStore.Get
            };
        if (DistanceWeapon.IsApplicable(itemType))
            return new DistanceWeapon(itemType, location)
            {
                ItemTypeFinder = _itemTypeStore.Get,
                Chargeable = chargeable
            };
        if (MagicWeapon.IsApplicable(itemType))
            return new MagicWeapon(itemType, location)
            {
                ItemTypeFinder = _itemTypeStore.Get,
                Chargeable = chargeable
            };

        if (ICumulative.IsApplicable(itemType))
        {
            if (ThrowableWeapon.IsApplicable(itemType))
                return new ThrowableWeapon(itemType, location, attributes);
            if (Ammo.IsApplicable(itemType)) return new Ammo(itemType, location, attributes);
        }

        return null;
    }
}