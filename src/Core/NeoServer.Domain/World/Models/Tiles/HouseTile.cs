using System.Collections.Generic;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Core.Houses;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.World.Models.Tiles;

/// <summary>
/// Represents a tile that belongs to a house
/// </summary>
public class HouseTile : DynamicTile
{
    public House House { get; private set; }

    public HouseTile(Coordinate coordinate, TileFlag flags, IEnumerable<IItem> items, House house)
        : base(coordinate, flags, items)
    {
        House = house;
    }

    public HouseTile(Coordinate coordinate, TileFlag flags, House house)
        : base(coordinate, flags)
    {
        House = house;
    }

    /// <summary>
    /// Check if a creature can walk on this house tile
    /// </summary>
    public override bool CanEnter(ICreature creature)
    {
        if (!base.CanEnter(creature))
            return false;

        // If it's not a player, use base logic
        if (creature is not IPlayer player)
            return true;

        // Check house permissions
        return House?.CanEnter(player.Id) ?? true;
    }

    /// <summary>
    /// Check if an item can be placed on this house tile
    /// </summary>
    public override bool CanAddItem(IItem item, ICreature by = null)
    {
        if (!base.CanAddItem(item, by))
            return false;

        // If it's not placed by a player, use base logic
        if (by is not IPlayer player)
            return true;

        // Check house edit permissions
        return House?.CanEdit(player.Id) ?? true;
    }

    /// <summary>
    /// Check if an item can be removed from this house tile
    /// </summary>
    public override bool CanRemoveItem(IItem item, ICreature by = null)
    {
        if (!base.CanRemoveItem(item, by))
            return false;

        // If it's not removed by a player, use base logic
        if (by is not IPlayer player)
            return true;

        // Check house edit permissions
        return House?.CanEdit(player.Id) ?? true;
    }

    /// <summary>
    /// Get the house associated with this tile
    /// </summary>
    public House GetHouse()
    {
        return House;
    }

    /// <summary>
    /// Set the house for this tile
    /// </summary>
    public void SetHouse(House house)
    {
        House = house;
    }
}
