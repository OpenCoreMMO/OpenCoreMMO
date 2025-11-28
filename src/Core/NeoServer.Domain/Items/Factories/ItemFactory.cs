using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Monster.Loot;
using NeoServer.Domain.Creatures.Monster.Summon;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Items.Events;
using NeoServer.Domain.Items.Items;
using NeoServer.Domain.Items.Items.Containers;
using NeoServer.Domain.Items.Items.Cumulatives;
using NeoServer.Domain.Items.Items.UsableItems;

namespace NeoServer.Domain.Items.Factories;

public class ItemFactory : IItemFactory
{
    public ItemFactory(
        IEnumerable<IItemEventSubscriber> itemEventSubscribers,
        DefenseEquipmentFactory defenseEquipmentFactory,
        WeaponFactory weaponFactory,
        ContainerFactory containerFactory,
        RuneFactory runeFactory,
        GroundFactory groundFactory,
        CumulativeFactory cumulativeFactory,
        GenericItemFactory genericItemFactory,
        IItemTypeStore itemTypeStore,
        ICoinTypeStore coinTypeStore)
    {
        ItemEventSubscribers = itemEventSubscribers;
        DefenseEquipmentFactory = defenseEquipmentFactory;
        WeaponFactory = weaponFactory;
        ContainerFactory = containerFactory;
        RuneFactory = runeFactory;
        GroundFactory = groundFactory;
        CumulativeFactory = cumulativeFactory;
        GenericItemFactory = genericItemFactory;
        ItemTypeStore = itemTypeStore;
        CoinTypeStore = coinTypeStore;
        Instance = this;
    }

    public static IItemFactory Instance { get; private set; }
    public IEnumerable<IItemEventSubscriber> ItemEventSubscribers { get; set; }
    public DefenseEquipmentFactory DefenseEquipmentFactory { get; set; }
    public WeaponFactory WeaponFactory { get; set; }
    public ContainerFactory ContainerFactory { get; set; }
    public RuneFactory RuneFactory { get; set; }
    public GroundFactory GroundFactory { get; set; }
    public CumulativeFactory CumulativeFactory { get; set; }
    public GenericItemFactory GenericItemFactory { get; set; }
    public IItemTypeStore ItemTypeStore { get; set; }
    public ICoinTypeStore CoinTypeStore { get; set; }

    public IItem CreateLootCorpse(
        ushort typeId,
        Location location,
        Loot loot,
        IThing killer)
    {
        if (!ItemTypeStore.TryGetValue(typeId, out var itemType)) return null;

        var createdItem = new LootContainer(itemType, location, loot);

        if (killer is Summon summonKiller)
            createdItem.Attributes.SetAttribute(
                ItemAttribute.CorpseOwner, summonKiller.Master.CreatureId);
        else if (killer is ICreature creatureKiller)
            createdItem.Attributes.SetAttribute(
                ItemAttribute.CorpseOwner, creatureKiller.CreatureId);

        SubscribeEvents(createdItem);

        EventAggregator.Invoke(new ItemCreatedEvent(createdItem));

        return createdItem;
    }

    public IItem Create(
        ushort typeId,
        Location location,
        int count = 1,
        IEnumerable<IItem> children = null)
    {
        var itemTypeAttributes = new Dictionary<ItemTypeAttribute, IConvertible> { { ItemTypeAttribute.Count, count } };

        var itemTypeCustomAttributes = new Dictionary<string, IConvertible>();
        var itemAttributes = new Dictionary<ItemAttribute, IConvertible>();
        var itemCustomAttributes = new Dictionary<string, IConvertible>();
        return Create(typeId, location, itemTypeAttributes, itemTypeCustomAttributes, itemAttributes,
            itemCustomAttributes, children);
    }

    public IItem Create(
        ushort typeId,
        Location location,
        IDictionary<ItemTypeAttribute, IConvertible> itemTypeAttributes = null,
        IDictionary<string, IConvertible> itemTypeCustomAttributes = null,
        IDictionary<ItemAttribute, IConvertible> itemAttributes = null,
        IDictionary<string, IConvertible> itemCustomAttributes = null,
        IEnumerable<IItem> children = null)
    {
        if (!ItemTypeStore.TryGetValue(typeId, out var itemType)) return null;

        var createdItem = CreateItem(itemType, location, itemTypeAttributes, itemAttributes, children);

        SetAttributes(itemTypeAttributes, itemTypeCustomAttributes, itemAttributes, itemCustomAttributes, createdItem);

        SubscribeEvents(createdItem);

        EventAggregator.Invoke(new ItemCreatedEvent(createdItem));
        
        return createdItem;
    }

    public IItem Create(
        IItemType itemType,
        Location location,
        IDictionary<ItemTypeAttribute, IConvertible> itemTypeAttributes = null,
        IDictionary<string, IConvertible> itemTypeCustomAttributes = null,
        IDictionary<ItemAttribute, IConvertible> itemAttributes = null,
        IDictionary<string, IConvertible> itemCustomAttributes = null,
        IEnumerable<IItem> children = null)
    {
        var createdItem = CreateItem(itemType, location, itemTypeAttributes, itemAttributes, children);

        SetAttributes(itemTypeAttributes, itemTypeCustomAttributes, itemAttributes, itemCustomAttributes, createdItem);

        SubscribeEvents(createdItem);

        EventAggregator.Invoke(new ItemCreatedEvent(createdItem));

        return createdItem;
    }

    public IEnumerable<Coin> CreateCoins(ulong amount)
    {
        var coinsToAdd = CoinCalculator.Calculate(CoinTypeStore.Map, amount);

        foreach (var coinToAdd in coinsToAdd)
        {
            var createdCoin = Create(coinToAdd.Item1, Location.Inventory(Slot.Backpack), null);
            if (createdCoin is not Coin newCoin) continue;
            newCoin.SetAmount(coinToAdd.Item2);

            EventAggregator.Invoke(new ItemCreatedEvent(newCoin));

            yield return newCoin;
        }
    }

    public IItem Create(
        string name,
        Location location,
        IDictionary<ItemTypeAttribute, IConvertible> itemTypeAttributes = null,
        IDictionary<string, IConvertible> itemTypeCustomAttributes = null,
        IDictionary<ItemAttribute, IConvertible> itemAttributes = null,
        IDictionary<string, IConvertible> itemCustomAttributes = null,
        IEnumerable<IItem> children = null)
    {
        var item = ItemTypeStore.All.FirstOrDefault(x =>
            x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        return item is null
            ? null
            : Create(item.ServerId, location, itemTypeAttributes, itemTypeCustomAttributes, itemAttributes,
                itemCustomAttributes, children);
    }

    private static void SetAttributes(
        IDictionary<ItemTypeAttribute, IConvertible> itemTypeAttributes,
        IDictionary<string, IConvertible> itemTypeCustomAttributes,
        IDictionary<ItemAttribute, IConvertible> itemAttributes,
        IDictionary<string, IConvertible> itemCustomAttributes,
        IItem createdItem)
    {
        if (Guard.IsNull(createdItem))
            return;

        if (!Guard.IsNull(itemTypeAttributes) && itemTypeAttributes.Any())
            createdItem.Metadata.Attributes.SetAttribute(itemTypeAttributes);

        if (!Guard.IsNull(itemTypeCustomAttributes) && itemTypeCustomAttributes.Any())
            createdItem.Metadata.Attributes.SetCustomAttribute(itemTypeCustomAttributes);

        if (!Guard.IsNull(itemAttributes) && itemAttributes.Any())
            createdItem.Attributes.SetAttribute(itemAttributes);

        if (!Guard.IsNull(itemCustomAttributes) && itemCustomAttributes.Any())
            createdItem.Attributes.SetCustomAttribute(itemCustomAttributes);

        if (!createdItem.Attributes.HasAttribute(ItemAttribute.Count))
            createdItem.Attributes.SetAttribute(ItemAttribute.Count, 1);
    }

    private void SubscribeEvents(IItem createdItem)
    {
        if (Guard.IsNull(createdItem)) return;

        if (ItemEventSubscribers is null) return;

        foreach (var gameSubscriber in ItemEventSubscribers.Where(x =>
                     x.GetType().IsAssignableTo(typeof(IGameEventSubscriber)))) //register game events first
            gameSubscriber.Subscribe(createdItem);

        foreach (var subscriber in ItemEventSubscribers.Where(x =>
                     !x.GetType().IsAssignableTo(typeof(IGameEventSubscriber)))) //than register server events
            subscriber.Subscribe(createdItem);
    }

    private IItem CreateItem(
        IItemType itemType,
        Location location,
        IDictionary<ItemTypeAttribute, IConvertible> itemTypeAttributes,
        IDictionary<ItemAttribute, IConvertible> itemAttributes,
        IEnumerable<IItem> children)
    {
        if (itemType.ServerId < 100) return null;

        if (itemType.Group == ItemGroup.Deprecated) return null;

        if (itemType.Attributes.GetAttribute(ItemTypeAttribute.Script) is { } script)
            if (ItemFromScriptFactory.Create(itemType, location, itemTypeAttributes, script) is { } instance)
                return instance;

        if (DefenseEquipmentFactory?.Create(itemType, location) is { } equipment) return equipment;
        if (WeaponFactory?.Create(itemType, location, itemAttributes) is { } weapon) return weapon;
        if (ContainerFactory?.Create(itemType, location, children) is { } container) return container;
        if (RuneFactory?.Create(itemType, location) is { } rune) return rune;
        if (GroundFactory?.Create(itemType, location) is { } ground) return ground;

        if (CumulativeFactory?.Create(itemType, location) is { } cumulative) return cumulative;

        if (LiquidPool.IsApplicable(itemType)) return new LiquidPool(itemType, location);
        if (MagicField.IsApplicable(itemType)) return new MagicField(itemType, location);
        if (FloorChanger.IsApplicable(itemType)) return new FloorChanger(itemType, location);

        if (TeleportItem.IsApplicable(itemType)) return new TeleportItem(itemType, location);

        if (Paper.IsApplicable(itemType))
            return itemType.ServerId switch
            {
                GameConstants.LABEL_SERVER_ID => new Label(itemType, location),
                GameConstants.LETTER_SERVER_ID => new Letter(itemType, location),
                _ => new Paper(itemType, location)
            };

        if (Sign.IsApplicable(itemType, itemAttributes)) return new Sign(itemType, location);

        if (UsableOnItem.IsApplicable(itemType))
        {
            if (FloorChangerUsableItem.IsApplicable(itemType))
                return new FloorChangerUsableItem(itemType, location);

            return new UsableOnItem(itemType, location);
        }

        return GenericItemFactory?.Create(itemType, location);
    }
}