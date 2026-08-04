using NeoServer.Data.InMemory.DataStores;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Parsers;
using NeoServer.Domain.Creatures.Monster.Loot;
using NeoServer.Domain.Items;
using NeoServer.Domain.Items.Bases;
using NeoServer.Domain.Items.Factories;
using NeoServer.Domain.Items.Items;
using NeoServer.Domain.Items.Items.Attributes;
using NeoServer.Domain.Items.Items.Containers;
using NeoServer.Domain.Items.Items.Containers.Container;
using NeoServer.Domain.Items.Items.Cumulatives;
using NeoServer.Domain.Items.Items.UsableItems;
using NeoServer.Domain.Items.Items.UsableItems.Runes;
using NeoServer.Domain.Items.Items.Weapons;

namespace NeoServer.Domain.Tests.Helpers;

public class ItemTestDataBuilder
{
    public static void LoadItemTypeAttributes(IItemType itemType,
        (ItemTypeAttribute, IConvertible)[] itemTypeAttributes = null)
    {
        itemTypeAttributes ??= [];
        foreach (var (itemTypeAttribute, value) in itemTypeAttributes)
            itemType.Attributes.SetAttribute(itemTypeAttribute, value);
    }

    public static void LoadItemAttributes(IItem item, (ItemAttribute, IConvertible)[] itemAttributes = null)
    {
        itemAttributes ??= [];
        foreach (var (itemAttribute, value) in itemAttributes)
            item.Attributes.SetAttribute(itemAttribute, value);
    }

    public static Container CreateContainer(byte capacity = 6, float weight = 0, string name = "bag",
        IEnumerable<IItem> children = null, ushort id = 200,
        (ItemTypeAttribute, IConvertible)[] itemTypeAttributes = null,
        (ItemAttribute, IConvertible)[] itemAttributes = null)
    {
        var itemType = new ItemType();
        itemType.SetId(id);
        itemType.SetClientId(id);
        itemType.SetName(name);
        itemType.SetArticle("a");
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Capacity, capacity);
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Weight, weight);
        itemType.SetFlag(ItemFlag.Movable);

        if (itemAttributes == null)
            itemAttributes =
            [
                (ItemAttribute.Count, 1)
            ];

        LoadItemTypeAttributes(itemType, itemTypeAttributes);

        var item = new Container(itemType, new Location(100, 100, 7), children);
        LoadItemAttributes(item, itemAttributes);
        return item;
    }

    public static Parcel CreateParcel(byte capacity = 6, float weight = 0, string name = "bag",
        IEnumerable<IItem> children = null, ushort id = 0,
        (ItemTypeAttribute, IConvertible)[] itemTypeAttributes = null,
        (ItemAttribute, IConvertible)[] itemAttributes = null)
    {
        var itemType = new ItemType();
        itemType.SetId(id);
        itemType.SetClientId(id);
        itemType.SetName(name);
        itemType.SetArticle("a");
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Capacity, capacity);
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Weight, weight);
        itemType.SetFlag(ItemFlag.Movable);

        if (itemAttributes == null)
            itemAttributes =
            [
                (ItemAttribute.Count, 1)
            ];

        LoadItemTypeAttributes(itemType, itemTypeAttributes);

        var item = new Parcel(itemType, new Location(100, 100, 7), children);
        LoadItemAttributes(item, itemAttributes);
        return item;
    }

    public static Container CreateLootContainer(byte capacity = 6, string name = "bag", Loot loot = null,
        (ItemTypeAttribute, IConvertible)[] itemTypeAttributes = null,
        (ItemAttribute, IConvertible)[] itemAttributes = null)
    {
        var itemType = new ItemType();
        itemType.SetName(name);
        itemType.SetClientId(15);
        itemType.SetId(15);
        itemType.SetArticle("a");
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Capacity, capacity);
        itemType.SetFlag(ItemFlag.Movable);

        if (itemAttributes == null)
            itemAttributes =
            [
                (ItemAttribute.Count, 1)
            ];

        LoadItemTypeAttributes(itemType, itemTypeAttributes);

        var item = new LootContainer(itemType, new Location(100, 100, 7), loot);
        LoadItemAttributes(item, itemAttributes);
        return item;
    }

    public static Container CreatePickupableContainer(byte capacity = 6, IEnumerable<IItem> children = null,
        bool backpack = false,
        (ItemTypeAttribute, IConvertible)[] itemTypeAttributes = null,
        (ItemAttribute, IConvertible)[] itemAttributes = null)
    {
        var itemType = new ItemType();
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Capacity, capacity);
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Weight, 20);
        itemType.Flags.Add(ItemFlag.Pickupable);
        itemType.Flags.Add(ItemFlag.Movable);
        itemType.SetClientId(5);
        itemType.SetId(5);
        if (backpack)
            itemType.Attributes.SetAttribute(ItemTypeAttribute.BodyPosition, "backpack");

        if (itemAttributes == null)
            itemAttributes =
            [
                (ItemAttribute.Count, 1)
            ];

        LoadItemTypeAttributes(itemType, itemTypeAttributes);

        var item = new Container(itemType, new Location(100, 100, 7), children?.ToList());
        LoadItemAttributes(item, itemAttributes);
        return item;
    }

    public static Container CreateBackpack(ushort id = 1, float weight = 20, List<IItem> items = null,
        (ItemTypeAttribute, IConvertible)[] itemTypeAttributes = null,
        (ItemAttribute, IConvertible)[] itemAttributes = null)
    {
        var itemType = new ItemType();
        itemType.SetClientId(id);
        itemType.SetId(id);
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Capacity, 20);
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Weight, weight);
        itemType.Flags.Add(ItemFlag.Pickupable);
        itemType.Flags.Add(ItemFlag.Movable);
        itemType.Attributes.SetAttribute(ItemTypeAttribute.BodyPosition, "backpack");

        if (itemAttributes == null)
            itemAttributes =
            [
                (ItemAttribute.Count, 1)
            ];

        LoadItemTypeAttributes(itemType, itemTypeAttributes);

        var item = new Container(itemType, new Location(100, 100, 7), items);
        LoadItemAttributes(item, itemAttributes);
        return item;
    }

    public static Locker.Locker CreateLocker(ushort id = 1, float weight = 20, List<IItem> items = null,
        (ItemTypeAttribute, IConvertible)[] itemTypeAttributes = null,
        (ItemAttribute, IConvertible)[] itemAttributes = null)
    {
        var itemType = new ItemType();
        itemType.SetClientId(id);
        itemType.SetId(id);
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Capacity, 20);
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Weight, weight);

        if (itemAttributes == null)
            itemAttributes =
            [
                (ItemAttribute.Count, 1)
            ];

        LoadItemTypeAttributes(itemType, itemTypeAttributes);

        var item = new Locker.Locker(itemType, new Location(100, 100, 7), items);
        LoadItemAttributes(item, itemAttributes);
        return item;
    }

    public static Container CreateMailInbox(ushort id = 2593, float weight = 20, List<IItem> items = null,
        (ItemTypeAttribute, IConvertible)[] itemTypeAttributes = null,
        (ItemAttribute, IConvertible)[] itemAttributes = null)
    {
        var itemType = new ItemType();
        itemType.SetClientId(id);
        itemType.SetId(id);
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Capacity, 20);
        itemType.Attributes.SetAttribute(ItemTypeAttribute.Weight, weight);

        if (itemAttributes == null)
            itemAttributes =
            [
                (ItemAttribute.Count, 1)
            ];

        LoadItemTypeAttributes(itemType, itemTypeAttributes);

        var item = new Container(itemType, new Location(100, 100, 7), items);
        LoadItemAttributes(item, itemAttributes);
        return item;
    }

    public static ICumulative CreateCumulativeItem(ushort id, byte amount = 1, string name = "item", string slot = null,
        float weight = 1,
        (ItemTypeAttribute, IConvertible)[] itemTypeAttributes = null,
        (ItemAttribute, IConvertible)[] itemAttributes = null)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetName(name);
        type.Attributes.SetAttribute(ItemTypeAttribute.BodyPosition, slot);
        type.Attributes.SetAttribute(ItemTypeAttribute.Weight, weight);
        type.Flags.Add(ItemFlag.Stackable);
        type.Flags.Add(ItemFlag.Pickupable);
        type.Flags.Add(ItemFlag.Movable);

        if (itemAttributes == null)
            itemAttributes =
            [
                (ItemAttribute.Count, amount)
            ];

        LoadItemTypeAttributes(type, itemTypeAttributes);

        var item = new Cumulative(type, new Location(100, 100, 7), amount);
        LoadItemAttributes(item, itemAttributes);
        return item;
    }

    public static Item CreateRegularItem(ushort id,
        (ItemTypeAttribute, IConvertible)[] itemTypeAttributes = null,
        (ItemAttribute, IConvertible)[] itemAttributes = null)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetName("item");

        if (itemAttributes == null)
            itemAttributes =
            [
                (ItemAttribute.Count, 1)
            ];

        LoadItemTypeAttributes(type, itemTypeAttributes);

        var item = new Item(type, new Location(100, 100, 7));
        LoadItemAttributes(item, itemAttributes);
        return item;
    }

    public static IItem CreateMoveableItem(ushort id,
        (ItemTypeAttribute, IConvertible)[] itemTypeAttributes = null,
        (ItemAttribute, IConvertible)[] itemAttributes = null)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetName("item");
        type.Flags.Add(ItemFlag.Movable);

        if (itemAttributes == null)
            itemAttributes =
            [
                (ItemAttribute.Count, 1)
            ];

        LoadItemTypeAttributes(type, itemTypeAttributes);

        var item = new MeleeWeapon(type, new Location(100, 100, 7));
        LoadItemAttributes(item, itemAttributes);
        return item;
    }

    public static IItemType CreateMoveableItemMetadata(ushort id)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetName("item");
        type.Flags.Add(ItemFlag.Movable);
        type.Attributes.SetAttribute(ItemTypeAttribute.Count, 1);
        return type;
    }

    public static IItem CreatePotion(ushort id,
        (ItemTypeAttribute, IConvertible)[] itemTypeAttributes = null,
        (ItemAttribute, IConvertible)[] itemAttributes = null)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetName("pot");
        type.Attributes.SetAttribute(ItemTypeAttribute.Weight, 10);
        type.Flags.Add(ItemFlag.Pickupable);
        type.SetFlag(ItemFlag.Movable);

        if (itemAttributes == null)
            itemAttributes =
            [
                (ItemAttribute.Count, 1)
            ];

        LoadItemTypeAttributes(type, itemTypeAttributes);

        var item = new HealingItem(type, new Location(100, 100, 7));
        LoadItemAttributes(item, itemAttributes);
        return item;
    }

    public static IEquipment CreateMagicWeapon(ushort id, bool twoHanded = false, byte charges = 0,
        (ItemTypeAttribute, IConvertible)[] itemTypeAttributes = null,
        (ItemAttribute, IConvertible)[] itemAttributes = null,
        Func<ushort, IItemType> itemTypeFinder = null)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetName("magic weapon");
        type.Attributes.SetAttribute(ItemTypeAttribute.Weight, 40);
        type.Attributes.SetAttribute(ItemTypeAttribute.BodyPosition, twoHanded ? "two-handed" : "weapon");
        type.Flags.Add(ItemFlag.Pickupable);
        type.Flags.Add(ItemFlag.Movable);

        if (itemAttributes == null)
            itemAttributes =
            [
                (ItemAttribute.Count, 1)
            ];

        LoadItemTypeAttributes(type, itemTypeAttributes);

        var item = new MagicWeapon(type, new Location(100, 100, 7))
        {
            Charges = charges > 0
                ? new ChargeCounter(charges, type.Attributes.GetAttribute<bool>(ItemTypeAttribute.ShowCharges))
                : null,
            ItemTypeFinder = itemTypeFinder
        };
        LoadItemAttributes(item, itemAttributes);
        return item;
    }

    public static IItem CreateWeaponItem(ushort id, string article = "a", string name = "item",
        string weaponType = "sword", bool twoHanded = false,
        byte charges = 0,
        (ItemTypeAttribute, IConvertible)[] itemTypeAttributes = null,
        (ItemAttribute, IConvertible)[] itemAttributes = null,
        Func<ushort, IItemType> itemTypeFinder = null,
        float weight = 40)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetArticle(article);
        type.SetName(name);
        type.Attributes.SetAttribute(ItemTypeAttribute.WeaponType, weaponType);
        type.Attributes.SetAttribute(ItemTypeAttribute.Weight, weight);
        type.Flags.Add(ItemFlag.Pickupable);
        type.Flags.Add(ItemFlag.Movable);
        type.Attributes.SetAttribute(ItemTypeAttribute.BodyPosition, twoHanded ? "two-handed" : "weapon");

        if (itemAttributes == null)
            itemAttributes =
            [
                (ItemAttribute.Count, 1)
            ];

        LoadItemTypeAttributes(type, itemTypeAttributes);

        var item = new MeleeWeapon(type, new Location(100, 100, 7))
        {
            Charges = charges > 0
                ? new ChargeCounter(charges, type.Attributes.GetAttribute<bool>(ItemTypeAttribute.ShowCharges))
                : null,
            ItemTypeFinder = itemTypeFinder
        };
        LoadItemAttributes(item, itemAttributes);
        return item;
    }

    public static IItem CreateDistanceWeapon(ushort id, bool twoHanded = false, byte charges = 0,
        (ItemTypeAttribute, IConvertible)[] itemTypeAttributes = null,
        (ItemAttribute, IConvertible)[] itemAttributes = null,
        Func<ushort, IItemType> itemTypeFinder = null)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetName("item");
        type.Attributes.SetAttribute(ItemTypeAttribute.WeaponType, "distance");
        type.Attributes.SetAttribute(ItemTypeAttribute.Weight, 40);
        type.Flags.Add(ItemFlag.Pickupable);
        type.Flags.Add(ItemFlag.Movable);
        type.Attributes.SetAttribute(ItemTypeAttribute.BodyPosition, twoHanded ? "two-handed" : "weapon");

        if (itemAttributes == null)
            itemAttributes =
            [
                (ItemAttribute.Count, 1)
            ];

        LoadItemTypeAttributes(type, itemTypeAttributes);

        var item = new DistanceWeapon(type, new Location(100, 100, 7))
        {
            Charges = charges > 0
                ? new ChargeCounter(charges, type.Attributes.GetAttribute<bool>(ItemTypeAttribute.ShowCharges))
                : null,
            ItemTypeFinder = itemTypeFinder
        };
        LoadItemAttributes(item, itemAttributes);
        return item;
    }

    public static IEquipment CreateThrowableDistanceItem(ushort id, byte amount = 1, int weight = 40,
        bool twoHanded = false,
        int range = 7, int breakChance = 0,
        (ItemTypeAttribute, IConvertible)[] itemTypeAttributes = null,
        (ItemAttribute, IConvertible)[] itemAttributes = null,
        Func<ushort, IItemType> itemTypeFinder = null)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetName("item");
        type.Attributes.SetAttribute(ItemTypeAttribute.WeaponType, "distance");
        type.Attributes.SetAttribute(ItemTypeAttribute.Weight, weight);
        type.Attributes.SetAttribute(ItemTypeAttribute.BodyPosition, "weapon");
        type.Attributes.SetAttribute(ItemTypeAttribute.Range, range);
        type.Attributes.SetAttribute(ItemTypeAttribute.Count, amount);
        type.Attributes.SetCustomAttribute("breakChance", breakChance);
        type.Flags.Add(ItemFlag.Pickupable);
        type.Flags.Add(ItemFlag.Movable);
        type.Flags.Add(ItemFlag.Stackable);

        if (itemAttributes == null)
            itemAttributes =
            [
                (ItemAttribute.Count, amount)
            ];

        LoadItemTypeAttributes(type, itemTypeAttributes);

        var item = new ThrowableWeapon(
            type,
            new Location(100, 100, 7),
            itemAttributes != null ? itemAttributes.ToDictionary() : null)
        {
            Charges = null,
            ItemTypeFinder = itemTypeFinder
        };

        LoadItemAttributes(item, itemAttributes);
        return item;
    }

    public static BodyDefenseEquipment CreateDefenseEquipmentItem(ushort id, string slot = "", ushort charges = 10,
        (ItemTypeAttribute, IConvertible)[] itemTypeAttributes = null,
        (ItemAttribute, IConvertible)[] itemAttributes = null,
        Func<ushort, IItemType> itemTypeFinder = null)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.Attributes.SetAttribute(ItemTypeAttribute.BodyPosition, slot);
        type.Attributes.SetAttribute(ItemTypeAttribute.Charges, charges);
        type.SetName("item");
        type.Flags.Add(ItemFlag.Pickupable);
        type.Flags.Add(ItemFlag.Movable);

        if (itemAttributes == null)
            itemAttributes =
            [
                (ItemAttribute.Count, 1)
            ];

        LoadItemTypeAttributes(type, itemTypeAttributes);
        type.SetGroupIfNone();

        var item = new BodyDefenseEquipment(type, new Location(100, 100, 7))
        {
            Charges = charges > 0
                ? new ChargeCounter(charges, type.Attributes.GetAttribute<bool>(ItemTypeAttribute.ShowCharges))
                : null,
            ItemTypeFinder = itemTypeFinder
        };
        LoadItemAttributes(item, itemAttributes);
        return item;
    }

    public static IItem CreateBodyEquipmentItem(ushort id, string slot, string weaponType = "", float weight = 40,
        (ItemTypeAttribute, IConvertible)[] itemTypeAttributes = null,
        (ItemAttribute, IConvertible)[] itemAttributes = null)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.Attributes.SetAttribute(ItemTypeAttribute.BodyPosition, slot);
        type.Attributes.SetAttribute(ItemTypeAttribute.WeaponType, weaponType);
        type.Attributes.SetAttribute(ItemTypeAttribute.Weight, weight);
        type.Flags.Add(ItemFlag.Pickupable);
        type.Flags.Add(ItemFlag.Movable);
        type.SetName("item");

        if (itemAttributes == null)
            itemAttributes =
            [
                (ItemAttribute.Count, 1)
            ];

        LoadItemTypeAttributes(type, itemTypeAttributes);
        type.SetGroupIfNone();

        var item = new BodyDefenseEquipment(type, new Location(100, 100, 7));
        LoadItemAttributes(item, itemAttributes);
        return item;
    }

    public static IItem CreateAmmo(ushort id, byte amount = 1,
        (ItemTypeAttribute, IConvertible)[] itemTypeAttributes = null,
        (ItemAttribute, IConvertible)[] itemAttributes = null,
        Func<ushort, IItemType> itemTypeFinder = null, float weight = 1)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetName("item");
        type.Attributes.SetAttribute(ItemTypeAttribute.WeaponType, "ammunition");
        type.Attributes.SetAttribute(ItemTypeAttribute.BodyPosition, "ammo");
        type.Attributes.SetAttribute(ItemTypeAttribute.Weight, weight);
        type.Flags.Add(ItemFlag.Stackable);
        type.Flags.Add(ItemFlag.Pickupable);
        type.Flags.Add(ItemFlag.Movable);

        if (itemAttributes == null)
            itemAttributes =
            [
                (ItemAttribute.Count, amount)
            ];

        LoadItemTypeAttributes(type, itemTypeAttributes);
        type.SetGroupIfNone();

        var item = new Ammo(
            type,
            new Location(100, 100, 7),
            itemAttributes != null ? itemAttributes.ToDictionary() : null)
        {
            Charges = null,
            ItemTypeFinder = itemTypeFinder
        };

        LoadItemAttributes(item, itemAttributes);
        return item;
    }

    public static Food CreateFood(ushort id, byte amount = 1, float weight = 13,
        (ItemTypeAttribute, IConvertible)[] itemTypeAttributes = null,
        (ItemAttribute, IConvertible)[] itemAttributes = null)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetName("meat");
        type.Attributes.SetAttribute(ItemTypeAttribute.Type, "food");
        type.Attributes.SetAttribute(ItemTypeAttribute.Duration, 30);
        type.Attributes.SetAttribute(ItemTypeAttribute.Sentence, "Munch.");
        type.Attributes.SetAttribute(ItemTypeAttribute.Weight, weight);
        type.Flags.Add(ItemFlag.Pickupable);
        type.Flags.Add(ItemFlag.Movable);
        type.Flags.Add(ItemFlag.Stackable);

        if (itemAttributes == null)
            itemAttributes =
            [
                (ItemAttribute.Count, amount)
            ];

        LoadItemTypeAttributes(type, itemTypeAttributes);
        type.SetGroupIfNone();

        var item = new Food(type, new Location(100, 100, 7), amount);
        LoadItemAttributes(item, itemAttributes);
        return item;
    }

    public static IItem CreateCoin(ushort id, byte amount = 1, uint multiplier = 1,
        (ItemTypeAttribute, IConvertible)[] itemTypeAttributes = null,
        (ItemAttribute, IConvertible)[] itemAttributes = null)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetName("coin");
        type.Attributes.SetAttribute(ItemTypeAttribute.Type, "coin");
        type.Attributes.SetAttribute(ItemTypeAttribute.Worth, multiplier);
        type.Attributes.SetAttribute(ItemTypeAttribute.Weight, 1);
        type.Flags.Add(ItemFlag.Pickupable);
        type.Flags.Add(ItemFlag.Movable);
        type.Flags.Add(ItemFlag.Stackable);

        if (itemAttributes == null)
            itemAttributes =
            [
                (ItemAttribute.Count, amount)
            ];

        LoadItemTypeAttributes(type, itemTypeAttributes);
        type.SetGroupIfNone();

        var item = new Coin(type, new Location(100, 100, 7), amount);
        LoadItemAttributes(item, itemAttributes);
        return item;
    }

    public static IItem CreateAttackRune(
        ushort id, DamageType damageType = DamageType.Energy,
        byte amount = 100, bool needTarget = true, ushort min = 100,
        ushort max = 100, IAreaEffectStore areaEffectStore = null,
        (ItemTypeAttribute, IConvertible)[] itemTypeAttributes = null,
        (ItemAttribute, IConvertible)[] itemAttributes = null)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetName("hmm");
        type.Attributes.SetAttribute(ItemTypeAttribute.Damage, DamageTypeParser.Parse(damageType));
        type.Attributes.SetAttribute(ItemTypeAttribute.Type, "rune");
        type.Attributes.SetAttribute(ItemTypeAttribute.NeedTarget, needTarget);
        type.Attributes.SetAttribute(ItemTypeAttribute.Count, amount);
        type.Attributes.SetCustomAttribute("x", new[] { min, max });
        type.Attributes.SetCustomAttribute("y", new[] { min, max });

        type.Flags.Add(ItemFlag.Stackable);
        type.Flags.Add(ItemFlag.Pickupable);
        type.Flags.Add(ItemFlag.Movable);

        if (itemAttributes == null)
            itemAttributes =
            [
                (ItemAttribute.Count, amount)
            ];

        LoadItemTypeAttributes(type, itemTypeAttributes);

        type.SetGroupIfNone();

        var factory = new RuneFactory();
        var item = (Rune)factory.Create(type, new Location(100, 100, 7));

        LoadItemAttributes(item, itemAttributes);
        return item;
    }

    public static IItem CreateTopItem(ushort id, byte topOrder,
        (ItemTypeAttribute, IConvertible)[] itemTypeAttributes = null,
        (ItemAttribute, IConvertible)[] itemAttributes = null)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetName("item");
        type.SetFlag(topOrder == 1 ? ItemFlag.AlwaysOnTop : ItemFlag.Bottom);

        itemAttributes ??=
        [
            (ItemAttribute.Count, 1)
        ];

        LoadItemTypeAttributes(type, itemTypeAttributes);

        var item = new Item(type, new Location(100, 100, 7));
        LoadItemAttributes(item, itemAttributes);
        return item;
    }

    public static IItem CreateUnpassableItem(ushort id,
        (ItemTypeAttribute, IConvertible)[] itemTypeAttributes = null,
        (ItemAttribute, IConvertible)[] itemAttributes = null)
    {
        var type = new ItemType();
        type.SetClientId(id);
        type.SetId(id);
        type.SetFlag(ItemFlag.Unpassable);
        type.SetName("item");

        if (itemAttributes == null)
            itemAttributes =
            [
                (ItemAttribute.Count, 1)
            ];

        LoadItemTypeAttributes(type, itemTypeAttributes);

        var item = new Item(type, new Location(100, 100, 7));
        LoadItemAttributes(item, itemAttributes);
        return item;
    }

    public static IItemTypeStore GetItemTypeStore(params IItemType[] itemTypes)
    {
        var itemTypeStore = new ItemTypeStore();
        foreach (var itemType in itemTypes) itemTypeStore.AddOrUpdate(itemType.ClientId, itemType);

        return itemTypeStore;
    }

    public static IItemTypeStore AddItemTypeStore(IItemTypeStore itemTypeStore, params IItemType[] itemTypes)
    {
        foreach (var itemType in itemTypes) itemTypeStore.AddOrUpdate(itemType.ClientId, itemType);
        return itemTypeStore;
    }
}