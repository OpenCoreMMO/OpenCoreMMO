using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Location.Structs.Helpers;

namespace NeoServer.Domain.World.Models.Tiles;

public class StaticTile : BaseTile, IStaticTile
{
    private IItem _topDownItemOnStack;

    public StaticTile(Coordinate coordinate, params IItem[] items) : this(
        new Location((ushort)coordinate.X, (ushort)coordinate.Y, (byte)coordinate.Z), items)
    {
    }

    public StaticTile(Location location, params IItem[] items)
    {
        SetNewLocation(location);
        Raw = GetRaw(items);
        ThingsCount = items.Length;
        AllItems = OrderItems(items);
    }

    public IItem[] Items { get; }

    public override int ThingsCount { get; }
    public byte[] Raw { get; }
    public override IItem TopTopItemOnStack => null;
    public override IItem TopDownItemOnStack => _topDownItemOnStack;
    public override ICreature TopCreatureOnStack => null;

    public override int ItemsCount => AllItems?.Length ?? 0;
    public override IItem[] AllItems { get; }

    public override bool TryGetStackPositionOfThing(IPlayer player, IThing thing, out byte stackPosition)
    {
        stackPosition = default;
        return false;
    }

    public override byte GetCreatureStackPositionIndex(IPlayer observer)
    {
        return 0;
    }

    public ushort[] AllClientIdItems
    {
        get
        {
            var itemsId = new ushort[Raw.Length / 2];
            var index = 0;

            for (var i = 0; i < Raw.Length; i += 2)
            {
                var final = i + 2;
                itemsId[index++] = BitConverter.ToUInt16(Raw.AsSpan()[i..final]);
            }

            return itemsId;
        }
    }

    public IStaticTile CreateClone(Location location)
    {
        foreach (var item in AllItems)
            item.SetNewLocation(location, true);

        return new StaticTile(location, AllItems);
    }

    public override IItem GetItemByIndex(int index)
    {
        if (index < 0 || index >= AllItems.Length) return null;
        return AllItems[index];
    }

    public byte[] GetRaw(IItem[] items)
    {
        var ground = new List<byte>();
        var top1 = new List<byte>();
        var downRawItems = new List<byte>();

        foreach (var item in items)
        {
            if (item is null) continue;

            if (item is IGround groundItem)
            {
                _topDownItemOnStack = groundItem;
                ground.AddRange(BitConverter.GetBytes(item.ClientId));
                continue;
            }

            if (item.IsAlwaysOnTop)
            {
                if (item.FloorDirection != default) FloorDirection = item.FloorDirection;

                _topDownItemOnStack = item;
                top1.AddRange(BitConverter.GetBytes(item.ClientId));
            }
            else
            {
                _topDownItemOnStack = item;
                downRawItems.InsertRange(0, BitConverter.GetBytes(item.ClientId));
            }

            SetTileFlags(item);
        }

        return ground.Concat(top1).Concat(downRawItems).ToArray();
    }

    private IItem[] OrderItems(IItem[] items)
    {
        if (items == null) return null;

        var orderedItems = new List<IItem>();

        // First, add ground items
        foreach (var item in items)
        {
            if (item is null) continue;
            if (item is IGround) orderedItems.Add(item);
        }

        // Then, add top items (IsAlwaysOnTop)
        foreach (var item in items)
        {
            if (item is null) continue;
            if (item.IsAlwaysOnTop && item is not IGround) orderedItems.Add(item);
        }

        // Finally, add down items (everything else)
        foreach (var item in items)
        {
            if (item is null) continue;
            if (!item.IsAlwaysOnTop && item is not IGround) orderedItems.Add(item);
        }

        return orderedItems.ToArray();
    }

    public override int GetHashCode()
    {
        return HashHelper.START
            .CombineHashCode(Raw);
    }
}