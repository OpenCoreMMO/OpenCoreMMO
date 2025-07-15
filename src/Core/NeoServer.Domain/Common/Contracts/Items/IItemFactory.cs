using NeoServer.Domain.Creatures.Monster.Loot;
using NeoServer.Domain.Items.Items.Cumulatives;

namespace NeoServer.Domain.Common.Contracts.Items;

public delegate void CreateItem(IItem item);

public interface IItemFactory : IFactory
{
    IItem Create(
        ushort typeId,
        Location.Structs.Location location,
        int count = 1,
        IEnumerable<IItem> children = null);

    IItem Create(
        ushort typeId,
        Location.Structs.Location location,
        IDictionary<ItemTypeAttribute, IConvertible> itemTypeAttributes,
        IDictionary<ItemAttribute, IConvertible> itemAttributes = null,
        IEnumerable<IItem> children = null);

    IItem Create(
        string name,
        Location.Structs.Location location,
        IDictionary<ItemTypeAttribute, IConvertible> itemTypeAttributes = null,
        IDictionary<ItemAttribute, IConvertible> itemAttributes = null,
        IEnumerable<IItem> children = null);

    IEnumerable<Coin> CreateCoins(ulong amount);
    IItem CreateLootCorpse(
        ushort typeId,
        Location.Structs.Location location,
        Loot loot,
        IThing killer);

    IItem Create(
        IItemType itemType,
        Location.Structs.Location location,
        IDictionary<ItemTypeAttribute, IConvertible> itemTypeAttributes = null,
        IDictionary<ItemAttribute, IConvertible> itemAttributes = null,
        IEnumerable<IItem> children = null);
}