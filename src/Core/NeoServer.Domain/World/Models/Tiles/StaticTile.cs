using System.Runtime.CompilerServices;
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
    private ushort[] _cachedClientIdItems;

    public StaticTile(Coordinate coordinate, uint flag = 0, params IItem[] items)
    {
        var location = new Location((ushort)coordinate.X, (ushort)coordinate.Y, (byte)coordinate.Z);
        SetNewLocation(location);
        Flags |= flag;
        Raw = GetRaw(items);
        ThingsCount = items.Length;
        AllItems = OrderItems(items);
    }

    public StaticTile(Location location, params IItem[] items)
    {
        SetNewLocation(location);
        Raw = GetRaw(items);
        ThingsCount = items.Length;
        AllItems = OrderItems(items);
    }

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
            if (_cachedClientIdItems != null) return _cachedClientIdItems;
            
            var itemsId = new ushort[Raw.Length / 2];
            var span = Raw.AsSpan();
            
            for (var i = 0; i < itemsId.Length; i++)
            {
                itemsId[i] = BitConverter.ToUInt16(span.Slice(i * 2, 2));
            }
            
            _cachedClientIdItems = itemsId;
            return _cachedClientIdItems;
        }
    }

    public IStaticTile CreateClone(Location location)
    {
        var clonedItems = new IItem[AllItems.Length];
        for (var i = 0; i < AllItems.Length; i++)
        {
            var item = AllItems[i];
            item.SetNewLocation(location, true);
            clonedItems[i] = item;
        }

        return new StaticTile(location, clonedItems);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override IItem GetItemByIndex(int index)
    {
        if ((uint)index >= (uint)AllItems.Length) return null;
        return AllItems[index];
    }

    public byte[] GetRaw(IItem[] items)
    {
        if (items == null || items.Length == 0) return Array.Empty<byte>();
        
        var capacity = items.Length * 2;
        var result = new byte[capacity];
        var groundPos = 0;
        var topPos = 0;
        var downPos = 0;
        
        // Count categories first
        var groundCount = 0;
        var topCount = 0;
        
        foreach (var item in items)
        {
            if (item is null) continue;
            if (item is IGround) groundCount++;
            else if (item.IsAlwaysOnTop) topCount++;
        }
        
        var topStart = groundCount * 2;
        var downStart = topStart + topCount * 2;
        
        foreach (var item in items)
        {
            if (item is null) continue;
            
            var clientIdBytes = BitConverter.GetBytes(item.ClientId);
            
            if (item is IGround groundItem)
            {
                _topDownItemOnStack = groundItem;
                result[groundPos++] = clientIdBytes[0];
                result[groundPos++] = clientIdBytes[1];
                continue;
            }
            
            if (item.IsAlwaysOnTop)
            {
                if (item.FloorDirection != default) FloorDirection = item.FloorDirection;
                _topDownItemOnStack = item;
                result[topStart + topPos++] = clientIdBytes[0];
                result[topStart + topPos++] = clientIdBytes[1];
            }
            else
            {
                _topDownItemOnStack = item;
                result[downStart + downPos++] = clientIdBytes[0];
                result[downStart + downPos++] = clientIdBytes[1];
            }
            
            SetTileFlags(item);
        }
        
        var actualSize = groundPos + topPos + downPos;
        if (actualSize < capacity)
        {
            Array.Resize(ref result, actualSize);
        }
        
        return result;
    }

    private IItem[] OrderItems(IItem[] items)
    {
        if (items == null || items.Length == 0) return Array.Empty<IItem>();
        
        var orderedItems = new IItem[items.Length];
        var groundIndex = 0;
        var topIndex = 0;
        var downIndex = 0;
        
        // Count categories in single pass
        foreach (var item in items)
        {
            if (item is null) continue;
            if (item is IGround) groundIndex++;
            else if (item.IsAlwaysOnTop) topIndex++;
            else downIndex++;
        }
        
        var topStart = groundIndex;
        var downStart = groundIndex + topIndex;
        
        groundIndex = 0;
        topIndex = topStart;
        downIndex = downStart;
        
        // Single pass to order items
        foreach (var item in items)
        {
            if (item is null) continue;
            
            if (item is IGround)
                orderedItems[groundIndex++] = item;
            else if (item.IsAlwaysOnTop)
                orderedItems[topIndex++] = item;
            else
                orderedItems[downIndex++] = item;
        }
        
        // Trim array if there were null items
        var actualCount = groundIndex + (topIndex - topStart) + (downIndex - downStart);
        if (actualCount < items.Length)
        {
            Array.Resize(ref orderedItems, actualCount);
        }
        
        return orderedItems;
    }

    public override int GetHashCode()
    {
        return HashHelper.START
            .CombineHashCode(Raw);
    }
}