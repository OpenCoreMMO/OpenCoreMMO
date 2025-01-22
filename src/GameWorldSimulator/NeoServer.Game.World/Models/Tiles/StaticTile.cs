using System;
using System.Collections.Generic;
using System.Linq;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Location.Structs.Helpers;

namespace NeoServer.Game.World.Models.Tiles;

public class StaticTile : BaseTile, IStaticTile
{
    private IItem _topItemOnStack;

    public StaticTile(Coordinate coordinate, params IItem[] items) : this(new Location((ushort)coordinate.X, (ushort)coordinate.Y, (byte)coordinate.Z), items)
    {
    }

    public StaticTile(Location location, params IItem[] items)
    {
        SetNewLocation(location);
        Raw = GetRaw(items);
        ThingsCount = items.Length;
        AllItems = items;
    }

    public override int ThingsCount { get; }
    public byte[] Raw { get; }
    public override IItem TopItemOnStack => _topItemOnStack;
    public override ICreature TopCreatureOnStack => null;
    public IItem[] Items { get; }

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
                itemsId[index++] = BitConverter.ToUInt16(Raw[i..final]);
            }

            return itemsId;
        }
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
                _topItemOnStack = groundItem;
                ground.AddRange(BitConverter.GetBytes(item.ClientId));
                continue;
            }

            if (item.IsAlwaysOnTop)
            {
                if (item.FloorDirection != default) FloorDirection = item.FloorDirection;

                _topItemOnStack = item;
                top1.AddRange(BitConverter.GetBytes(item.ClientId));
            }
            else
            {
                _topItemOnStack = item;
                downRawItems.InsertRange(0, BitConverter.GetBytes(item.ClientId));
            }

            SetTileFlags(item);
        }

        return ground.Concat(top1).Concat(downRawItems).ToArray();
    }

    public override int GetHashCode()
    {
        return HashHelper.START
            .CombineHashCode(Raw);
    }

    public IStaticTile CreateClone(Location location)
    {
        foreach (var item in AllItems)
            item.SetNewLocation(location, force: true);

        return new StaticTile(location, AllItems);
    }
}