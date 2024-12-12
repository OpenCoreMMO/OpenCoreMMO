using System;
using System.Collections.Generic;
using System.Linq;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Game.Common.Contracts.Items.Types.Body;
using NeoServer.Game.Common.Contracts.Items.Types.Runes;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Parsers;
using NeoServer.Game.Item;
using NeoServer.Game.Item.Factories;
using NeoServer.Game.Item.Items;
using NeoServer.Game.Item.Items.Attributes;
using NeoServer.Game.Item.Items.Containers;
using NeoServer.Game.Item.Items.Cumulatives;
using NeoServer.Game.Item.Items.UsableItems;
using NeoServer.Game.Item.Items.Weapons;
using NeoServer.BuildingBlocks.Infrastructure.Data.InMemory;

namespace NeoServer.Game.Tests.Helpers;

public class ItemTestData
{
    public static Container CreateContainer(byte capacity = 6, float weight = 0, string name = "bag",
        IEnumerable<IItem> children = null, ushort id = 0)
    {
        var itemType = new ItemType();

        itemType.SetId(id);
        itemType.SetClientId(id);
        itemType.SetName(name);
        itemType.SetArticle("a");
        itemType.Attributes.SetAttribute(ItemAttribute.Capacity, capacity);
        itemType.Attributes.SetAttribute(ItemAttribute.Weight, weight);
        itemType.SetFlag(ItemFlag.Movable);

        return new Container(itemType, new Location(100, 100, 7), children);
    }

    public static Container CreateLootContainer(byte capacity = 6, string name = "bag", ILoot loot = null)
    {
        var itemType = new ItemType();
        itemType.SetName(name);
        itemType.SetArticle("a");
        itemType.Attributes.SetAttribute(ItemAttribute.Capacity, capacity);
        itemType.SetFlag(ItemFlag.Movable);

        return new LootContainer(itemType, new Location(100, 100, 7), loot);
    }

    public static Container CreatePickupableContainer(byte capacity = 6, IEnumerable<IItem> children = null,
        bool backpack = false)
    {
        var itemType = new ItemType();
        itemType.Attributes.SetAttribute(ItemAttribute.Capacity, capacity);
        itemType.Attributes.SetAttribute(ItemAttribute.Weight, 20);

        itemType.Flags.Add(ItemFlag.Pickupable);
        itemType.Flags.Add(ItemFlag.Movable);

        if (backpack)
            itemType.Attributes.SetAttribute(ItemAttribute.BodyPosition, "backpack");

        return new Container(itemType, new Location(100, 100, 7), children?.ToList());
    }

    public static Container CreateBackpack(ushort id = 1, float weight = 20, List<IItem> items = null)
    {
        var itemType = new ItemType();
        itemType.SetClientId(id);
        itemType.SetId(id);
        itemType.Attributes.SetAttribute(ItemAttribute.Capacity, 20);
        itemType.Attributes.SetAttribute(ItemAttribute.Weight, weight);
        itemType.Flags.Add(ItemFlag.Pickupable);
        itemType.Flags.Add(ItemFlag.Movable);

        itemType.Attributes.SetAttribute(ItemAttribute.BodyPosition, "backpack");

        return new Container(itemType, new Location(100, 100, 7), items);
    }

    public static Depot CreateDepot(ushort id = 1, float weight = 20, List<IItem> items = null)
    {
        var itemType = new ItemType();
        itemType.SetClientId(id);
        itemType.SetId(id);
        itemType.Attributes.SetAttribute(ItemAttribute.Capacity, 20);
        itemType.Attributes.SetAttribute(ItemAttribute.Weight, weight);

        return new Depot(itemType, new Location(100, 100, 7), items);
    }

    public static ICumulative CreateCumulativeItem(ushort id, byte amount, string name = "item", string slot = null,
        float weight = 1)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetName(name);

        type.Attributes.SetAttribute(ItemAttribute.BodyPosition, slot);
        type.Attributes.SetAttribute(ItemAttribute.Weight, weight);

        type.Flags.Add(ItemFlag.Stackable);
        type.Flags.Add(ItemFlag.Pickupable);
        type.Flags.Add(ItemFlag.Movable);

        return new Cumulative(type, new Location(100, 100, 7), amount);
    }

    public static Game.Item.Bases.Item CreateRegularItem(ushort id)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetName("item");

        return new Game.Item.Bases.Item(type, new Location(100, 100, 7));
    }

    public static IItem CreateMoveableItem(ushort id)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);

        type.SetName("item");

        type.Flags.Add(ItemFlag.Movable);

        return new MeleeWeapon(type, new Location(100, 100, 7));
    }

    public static IItem CreatePot(ushort id,
        (ItemAttribute, IConvertible)[] attributes = null)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetName("pot");
        type.Attributes.SetAttribute(ItemAttribute.Weight, 10);
        type.Flags.Add(ItemFlag.Pickupable);
        type.SetFlag(ItemFlag.Movable);

        attributes ??= Array.Empty<(ItemAttribute, IConvertible)>();
        foreach (var (attributeType, value) in attributes) type.Attributes.SetAttribute(attributeType, value);

        return new HealingItem(type, new Location(100, 100, 7),
            attributes.ToDictionary(x => x.Item1, x => x.Item2));
    }

    public static IEquipment CreateMagicalWeapon(ushort id, bool twoHanded = false, byte charges = 0,
        (ItemAttribute, IConvertible)[] attributes = null, Func<ushort, IItemType> itemTypeFinder = null)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetName("magic weapon");
        type.Attributes.SetAttribute(ItemAttribute.Weight, 40);

        type.Attributes.SetAttribute(ItemAttribute.BodyPosition, twoHanded ? "two-handed" : "weapon");
        type.Flags.Add(ItemFlag.Pickupable);
        type.Flags.Add(ItemFlag.Movable);

        attributes ??= Array.Empty<(ItemAttribute, IConvertible)>();
        foreach (var (attributeType, value) in attributes) type.Attributes.SetAttribute(attributeType, value);

        return new MagicalWeapon(type, new Location(100, 100, 7))
        {
            Chargeable = charges > 0
                ? new Chargeable(charges, type.Attributes.GetAttribute<bool>(ItemAttribute.ShowCharges))
                : null,
            ItemTypeFinder = itemTypeFinder
        };
    }

    public static IItem CreateWeaponItem(ushort id, string article = "a", string name = "item",
        string weaponType = "sword", bool twoHanded = false,
        byte charges = 0,
        (ItemAttribute, IConvertible)[] attributes = null, Func<ushort, IItemType> itemTypeFinder = null,
        float weight = 40)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetArticle(article);
        type.SetName(name);
        type.Attributes.SetAttribute(ItemAttribute.WeaponType, weaponType);
        type.Attributes.SetAttribute(ItemAttribute.Weight, weight);
        type.Flags.Add(ItemFlag.Pickupable);
        type.Flags.Add(ItemFlag.Movable);

        type.Attributes.SetAttribute(ItemAttribute.BodyPosition, twoHanded ? "two-handed" : "weapon");

        attributes ??= Array.Empty<(ItemAttribute, IConvertible)>();
        foreach (var (attributeType, value) in attributes) type.Attributes.SetAttribute(attributeType, value);

        return new MeleeWeapon(type, new Location(100, 100, 7))
        {
            Chargeable = charges > 0
                ? new Chargeable(charges, type.Attributes.GetAttribute<bool>(ItemAttribute.ShowCharges))
                : null,
            ItemTypeFinder = itemTypeFinder
        };
    }

    public static IItem CreateDistanceWeapon(ushort id, bool twoHanded = false, byte charges = 0,
        (ItemAttribute, IConvertible)[] attributes = null, Func<ushort, IItemType> itemTypeFinder = null)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetName("item");
        type.Attributes.SetAttribute(ItemAttribute.WeaponType, "distance");
        type.Attributes.SetAttribute(ItemAttribute.Weight, 40);

        type.Flags.Add(ItemFlag.Pickupable);
        type.Flags.Add(ItemFlag.Movable);

        type.Attributes.SetAttribute(ItemAttribute.BodyPosition, twoHanded ? "two-handed" : "weapon");

        attributes ??= Array.Empty<(ItemAttribute, IConvertible)>();
        foreach (var (attributeType, value) in attributes) type.Attributes.SetAttribute(attributeType, value);

        return new DistanceWeapon(type, new Location(100, 100, 7))
        {
            Chargeable = charges > 0
                ? new Chargeable(charges, type.Attributes.GetAttribute<bool>(ItemAttribute.ShowCharges))
                : null,
            ItemTypeFinder = itemTypeFinder
        };
    }

    public static IEquipment CreateThrowableDistanceItem(ushort id, byte amount = 1, int weight = 40,
        bool twoHanded = false,
        int range = 7, int breakChance = 0,
        (ItemAttribute, IConvertible)[] attributes = null, Func<ushort, IItemType> itemTypeFinder = null)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetName("item");

        type.Attributes.SetAttribute(ItemAttribute.WeaponType, "distance");
        type.Attributes.SetAttribute(ItemAttribute.Weight, weight);
        type.Attributes.SetAttribute(ItemAttribute.BodyPosition, "weapon");
        type.Attributes.SetAttribute(ItemAttribute.Range, range);
        type.Attributes.SetCustomAttribute("breakChance", breakChance);

        type.Flags.Add(ItemFlag.Pickupable);
        type.Flags.Add(ItemFlag.Movable);
        type.Flags.Add(ItemFlag.Stackable);

        attributes ??= Array.Empty<(ItemAttribute, IConvertible)>();
        foreach (var (attributeType, value) in attributes) type.Attributes.SetAttribute(attributeType, value);

        return new ThrowableWeapon(type, new Location(100, 100, 7), amount)
        {
            Chargeable = null,
            ItemTypeFinder = itemTypeFinder
        };
    }

    public static IDefenseEquipment CreateDefenseEquipmentItem(ushort id, string slot = "", ushort charges = 10,
        (ItemAttribute, IConvertible)[] attributes = null, Func<ushort, IItemType> itemTypeFinder = null)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.Attributes.SetAttribute(ItemAttribute.BodyPosition, slot);
        type.Attributes.SetAttribute(ItemAttribute.Charges, charges);
        type.SetName("item");
        type.Flags.Add(ItemFlag.Pickupable);
        type.Flags.Add(ItemFlag.Movable);

        attributes ??= Array.Empty<(ItemAttribute, IConvertible)>();
        foreach (var (attributeType, value) in attributes) type.Attributes.SetAttribute(attributeType, value);

        type.SetGroupIfNone();

        return new BodyDefenseEquipment(type, new Location(100, 100, 7))
        {
            Chargeable = charges > 0
                ? new Chargeable(charges, type.Attributes.GetAttribute<bool>(ItemAttribute.ShowCharges))
                : null,
            ItemTypeFinder = itemTypeFinder
        };
    }

    public static IItem CreateBodyEquipmentItem(ushort id, string slot, string weaponType = "", float weight = 40)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.Attributes.SetAttribute(ItemAttribute.BodyPosition, slot);
        type.Attributes.SetAttribute(ItemAttribute.WeaponType, weaponType);
        type.Attributes.SetAttribute(ItemAttribute.Weight, weight);
        type.Flags.Add(ItemFlag.Pickupable);
        type.Flags.Add(ItemFlag.Movable);

        type.SetName("item");

        type.SetGroupIfNone();

        return new BodyDefenseEquipment(type, new Location(100, 100, 7));
    }

    public static IItem CreateAmmo(ushort id, byte amount, (ItemAttribute, IConvertible)[] attributes = null,
        Func<ushort, IItemType> itemTypeFinder = null, float weight = 1)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetName("item");
        type.Attributes.SetAttribute(ItemAttribute.WeaponType, "ammunition");
        type.Attributes.SetAttribute(ItemAttribute.BodyPosition, "ammo");
        type.Attributes.SetAttribute(ItemAttribute.Weight, weight);
        type.Flags.Add(ItemFlag.Stackable);
        type.Flags.Add(ItemFlag.Pickupable);
        type.Flags.Add(ItemFlag.Movable);

        attributes ??= Array.Empty<(ItemAttribute, IConvertible)>();
        foreach (var (attributeType, value) in attributes) type.Attributes.SetAttribute(attributeType, value);

        type.SetGroupIfNone();

        return new Ammo(type, new Location(100, 100, 7), amount)
        {
            Chargeable = null,
            ItemTypeFinder = itemTypeFinder
        };
    }

    public static Food CreateFood(ushort id, byte amount = 1, float weight = 13)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetName("meat");
        type.Attributes.SetAttribute(ItemAttribute.Type, "food");
        type.Attributes.SetAttribute(ItemAttribute.Duration, 30);
        type.Attributes.SetAttribute(ItemAttribute.Sentence, "Munch.");
        type.Attributes.SetAttribute(ItemAttribute.Weight, weight);

        type.Flags.Add(ItemFlag.Pickupable);
        type.Flags.Add(ItemFlag.Movable);
        type.Flags.Add(ItemFlag.Stackable);

        type.SetGroupIfNone();

        return new Food(type, new Location(100, 100, 7), amount);
    }

    public static IItem CreateCoin(ushort id, byte amount, uint multiplier)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetName("coin");
        type.Attributes.SetAttribute(ItemAttribute.Type, "coin");
        type.Attributes.SetAttribute(ItemAttribute.Worth, multiplier);
        type.Attributes.SetAttribute(ItemAttribute.Weight, 1);

        type.Flags.Add(ItemFlag.Pickupable);
        type.Flags.Add(ItemFlag.Movable);
        type.Flags.Add(ItemFlag.Stackable);

        type.SetGroupIfNone();

        return new Coin(type, new Location(100, 100, 7), amount);
    }

    public static IAttackRune CreateAttackRune(ushort id, DamageType damageType = DamageType.Energy,
        byte amount = 100,
        bool needTarget = true, ushort min = 100, ushort max = 100, IAreaEffectStore areaEffectStore = null)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetName("hmm");
        type.Attributes.SetAttribute(ItemAttribute.Damage, DamageTypeParser.Parse(damageType));
        type.Attributes.SetAttribute(ItemAttribute.Type, "rune");
        type.Attributes.SetAttribute(ItemAttribute.NeedTarget, needTarget);
        type.Attributes.SetCustomAttribute("x", new[] { min.ToString(), max.ToString() });
        type.Attributes.SetCustomAttribute("y", new[] { min.ToString(), max.ToString() });

        type.Flags.Add(ItemFlag.Stackable);
        type.Flags.Add(ItemFlag.Pickupable);
        type.Flags.Add(ItemFlag.Movable);

        var attributes = new Dictionary<ItemAttribute, IConvertible>
        {
            [ItemAttribute.Count] = amount
        };

        areaEffectStore ??= new AreaEffectStore();

        type.SetGroupIfNone();

        var factory = new RuneFactory(areaEffectStore);
        return (IAttackRune)factory.Create(type, new Location(100, 100, 7), attributes);
    }

    public static IItem CreateTopItem(ushort id, byte topOrder)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetName("item");

        if (topOrder == 1)
            type.SetFlag(ItemFlag.AlwaysOnTop);
        else
            type.SetFlag(ItemFlag.Bottom);

        return new Game.Item.Bases.Item(type, new Location(100, 100, 7));
    }

    public static IItem CreateUnpassableItem(ushort id)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetName("item");

        type.SetFlag(ItemFlag.Unpassable);

        return new Game.Item.Bases.Item(type, new Location(100, 100, 7));
    }

    public static IItemTypeStore GetItemTypeStore(params IItemType[] itemTypes)
    {
        var itemTypeStore = new ItemTypeStore();
        foreach (var itemType in itemTypes) itemTypeStore.Add(itemType.ClientId, itemType);

        return itemTypeStore;
    }

    public static IItemTypeStore AddItemTypeStore(IItemTypeStore itemTypeStore, params IItemType[] itemTypes)
    {
        foreach (var itemType in itemTypes) itemTypeStore.Add(itemType.ClientId, itemType);

        return itemTypeStore;
    }
}