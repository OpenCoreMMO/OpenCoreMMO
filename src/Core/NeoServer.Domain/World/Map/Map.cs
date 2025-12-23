using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.World.Events;
using NeoServer.Domain.World.Models;
using NeoServer.Domain.World.Models.Tiles;
using MinMax = NeoServer.Domain.Common.MinMax;

namespace NeoServer.Domain.World.Map;

/// <summary>
///     Represents the map within the world structure which provides access and operations
///     related to tiles, creatures, and spectators in the game world.
/// </summary>
public class Map : IMap
{
    private readonly CylinderOperation _cylinderOperation;
    private readonly IEventAggregator _eventAggregator;
    private readonly World _world;

    public Map(World world, IEventAggregator eventAggregator)
    {
        _world = world;
        _eventAggregator = eventAggregator;
        _cylinderOperation = new CylinderOperation(this);
    }

    public ITile this[Location location] => _world.TryGetTile(ref location, out var tile) ? tile : null;

    public ITile this[ushort x, ushort y, byte z] => this[new Location(x, y, z)];

    /// <summary>
    ///     Retrieves the tile located at the specified position within the map.
    /// </summary>
    /// <param name="location">The location specifying the coordinates of the desired tile.</param>
    /// <returns>Returns the <see cref="ITile" /> corresponding to the provided location.</returns>
    public ITile GetTile(Location location)
    {
        return this[location];
    }


    /// <summary>
    ///     Transfers a creature from one sector to another within the map.
    /// </summary>
    /// <param name="creature">The creature to be moved between sectors.</param>
    /// <param name="fromLocation">The initial location of the creature.</param>
    /// <param name="toLocation">The destination location where the creature will be moved.</param>
    public void SwapCreatureBetweenSectors(ICreature creature, Location fromLocation, Location toLocation)
    {
        _world.SwapCreatureBetweenSectors(creature, fromLocation, toLocation);
    }

    /// <summary>
    ///     Determines whether the current location is within a valid range of the target location
    ///     based on the specified start location and path search parameters.
    /// </summary>
    /// <param name="start">The starting location for the range check.</param>
    /// <param name="current">The current location to validate.</param>
    /// <param name="target">The target location to check the range against.</param>
    /// <param name="fpp">The path search parameters that define range constraints.</param>
    /// <returns>Returns true if the current location is within the valid range of the target location; otherwise, false.</returns>
    public bool IsInRange(Location start, Location current, Location target, FindPathParams fpp)
    {
        if (fpp.FullPathSearch)
        {
            if (current.X > target.X + fpp.MaxTargetDist) return false;

            if (current.X < target.X - fpp.MaxTargetDist) return false;

            if (current.Y > target.Y + fpp.MaxTargetDist) return false;

            if (current.Y < target.Y - fpp.MaxTargetDist) return false;
        }
        else
        {
            var dx = start.GetSqmDistanceX(target, false);

            var dxMax = dx >= 0 ? fpp.MaxTargetDist : 0;
            if (current.X > target.X + dxMax) return false;

            var dxMin = dx <= 0 ? fpp.MaxTargetDist : 0;
            if (current.X < target.X - dxMin) return false;

            var dy = start.GetSqmDistanceY(target, false);

            var dyMax = dy >= 0 ? fpp.MaxTargetDist : 0;
            if (current.Y > target.Y + dyMax) return false;

            var dyMin = dy <= 0 ? fpp.MaxTargetDist : 0;
            if (current.Y < target.Y - dyMin) return false;
        }

        return true;
    }

    /// <summary>
    ///     Retrieves the immediate destination tile based on the specified tile and its associated floor change behavior or
    ///     direction.
    /// </summary>
    /// <param name="tile">The tile for which the destination is to be determined.</param>
    /// <returns>
    ///     Returns the immediate destination tile if a valid floor destination exists; otherwise, returns the original
    ///     tile.
    /// </returns>
    public ITile GetTileDestination(ITile tile)
    {
        if (tile is not IDynamicTile toTile) return tile;

        bool HasFloorDestination(ITile walkableTile, FloorChangeDirection direction)
        {
            return walkableTile is IDynamicTile walkable && walkable.FloorDirection == direction;
        }

        var x = tile.Location.X;
        var y = tile.Location.Y;
        var z = tile.Location.Z;

        if (HasFloorDestination(tile, FloorChangeDirection.Down))
        {
            z++;

            var southDownTile = this[x, (ushort)(y - 1), z];

            if (HasFloorDestination(southDownTile, FloorChangeDirection.SouthAlternative))
            {
                y -= 2;
                return this[x, y, z] ?? tile;
            }

            var eastDownTile = this[(ushort)(x - 1), y, z];

            if (HasFloorDestination(eastDownTile, FloorChangeDirection.EastAlternative))
            {
                x -= 2;
                return this[x, y, z] ?? tile;
            }

            var downTile = this[x, y, z];

            if (downTile == null) return tile;

            if (HasFloorDestination(downTile, FloorChangeDirection.North)) ++y;
            if (HasFloorDestination(downTile, FloorChangeDirection.South)) --y;
            if (HasFloorDestination(downTile, FloorChangeDirection.SouthAlternative)) y -= 2;
            if (HasFloorDestination(downTile, FloorChangeDirection.East)) --x;
            if (HasFloorDestination(downTile, FloorChangeDirection.EastAlternative)) x -= 2;
            if (HasFloorDestination(downTile, FloorChangeDirection.West)) ++x;

            return this[x, y, z] ?? tile;
        }

        if (toTile.FloorDirection != default) //has any floor destination check
        {
            z--;

            if (HasFloorDestination(tile, FloorChangeDirection.North)) --y;
            if (HasFloorDestination(tile, FloorChangeDirection.South)) ++y;
            if (HasFloorDestination(tile, FloorChangeDirection.SouthAlternative)) y += 2;
            if (HasFloorDestination(tile, FloorChangeDirection.East)) ++x;
            if (HasFloorDestination(tile, FloorChangeDirection.EastAlternative)) x += 2;
            if (HasFloorDestination(tile, FloorChangeDirection.West)) --x;

            return this[x, y, z] ?? tile;
        }

        return tile;
    }

    /// <summary>
    ///     Retrieves a collection of spectators within the vicinity of a specified location.
    ///     Optionally, the search can be limited to include only player creatures.
    /// </summary>
    /// <param name="fromLocation">The starting location to search for spectators.</param>
    /// <param name="onlyPlayers">
    ///     A boolean indicating whether to include only player creatures in the results. Defaults to
    ///     false.
    /// </param>
    /// <returns>A collection of creatures that are spectators at or near the specified location.</returns>
    public HashSet<ICreature> GetSpectators(Location fromLocation, bool onlyPlayers = false)
    {
        return GetSpectators(fromLocation, fromLocation, onlyPlayers);
    }

    /// <summary>
    ///     Retrieves the creatures viewing the area between the specified locations, optionally filtering for players only.
    /// </summary>
    /// <param name="fromLocation">The initial location to determine the spectators.</param>
    /// <param name="toLocation">The target location to expand the spectator search area.</param>
    /// <param name="onlyPlayer">Indicates whether only player creatures should be included in the results.</param>
    /// <returns>Returns a set of creatures that can observe the area between the specified locations.</returns>
    public HashSet<ICreature> GetSpectators(Location fromLocation, Location toLocation, bool onlyPlayer = false)
    {
        var locationsAreNear = fromLocation.SameFloorAs(toLocation) &&
                               fromLocation.GetSqmDistanceX(toLocation) <= (int)MapViewPort.MaxViewPortX &&
                               fromLocation.GetSqmDistanceY(toLocation) <= (int)MapViewPort.MaxViewPortY;

        if (locationsAreNear)
        {
            var minRangeX = (int)MapViewPort.MaxViewPortX;
            var maxRangeX = (int)MapViewPort.MaxViewPortX;
            var minRangeY = (int)MapViewPort.MaxViewPortY;
            var maxRangeY = (int)MapViewPort.MaxViewPortY;

            if (fromLocation.Y > toLocation.Y) ++minRangeY;
            else if (fromLocation.Y < toLocation.Y) ++maxRangeY;

            if (fromLocation.X < toLocation.X) ++maxRangeX;
            else if (fromLocation.X > toLocation.X) ++minRangeX;

            var search = new SpectatorSearch(ref fromLocation, true, minRangeX, minRangeY: minRangeY,
                maxRangeX: maxRangeX, maxRangeY: maxRangeY, onlyPlayers: onlyPlayer);
            return _world.QuerySpectators(ref search).ToHashSet();
        }

        var oldSpecs = GetSpectators(fromLocation);
        var newSpecs = GetSpectators(toLocation);
        oldSpecs.UnionWith(newSpecs);

        return oldSpecs;
    }

    /// <summary>
    ///     Retrieves all players located within the specified position zone.
    /// </summary>
    /// <param name="location">The position zone to search for players.</param>
    /// <returns>A set of players found within the given position zone.</returns>
    public HashSet<ICreature> GetPlayersAtPositionZone(Location location)
    {
        return GetCreaturesAtPositionZone(location, true);
    }

    /// <summary>
    ///     Retrieves the set of creatures present in the zone defined by the specified source and target locations.
    ///     This includes creatures located at both the initial location and the target location.
    /// </summary>
    /// <param name="location">The starting location of the zone to query for creatures.</param>
    /// <param name="toLocation">The destination location of the zone to query for creatures.</param>
    /// <returns>Returns a set of creatures present within the specified zone.</returns>
    public HashSet<ICreature> GetCreaturesAtPositionZone(Location location, Location toLocation)
    {
        if (location == toLocation) return GetCreaturesAtPositionZone(location);

        var fromSpectators = GetCreaturesAtPositionZone(location);
        var toSpectators = GetCreaturesAtPositionZone(toLocation);

        var spectators = new List<ICreature>(fromSpectators.Count + toSpectators.Count);

        spectators.AddRange(fromSpectators);
        spectators.AddRange(toSpectators);
        return spectators.ToHashSet();
    }

    /// <summary>
    ///     Retrieves a collection of creatures present within the specified position zone on the map.
    /// </summary>
    /// <param name="location">The location defining the position zone to inspect.</param>
    /// <param name="onlyPlayers">
    ///     A boolean indicator specifying whether to include only players. If set to true, only player
    ///     creatures are included; otherwise, all creatures are returned.
    /// </param>
    /// <returns>Returns a HashSet of creatures located within the specified position zone.</returns>
    public HashSet<ICreature> GetCreaturesAtPositionZone(Location location, bool onlyPlayers = false)
    {
        return GetSpectators(location, onlyPlayers);
    }

    /// <summary>
    ///     Retrieves the set of creatures that are spectators of a specified location, based on given parameters.
    /// </summary>
    /// <param name="location">The location for which to find spectators.</param>
    /// <param name="multifloor">Indicates whether to include spectators from multiple floors.</param>
    /// <param name="onlyPlayers">Specifies whether to include only players as spectators.</param>
    /// <param name="rangeX">The minimum and maximum range on the X-axis to calculate the spectator area.</param>
    /// <param name="rangeY">The minimum and maximum range on the Y-axis to calculate the spectator area.</param>
    /// <returns>Returns a set of creatures that meet the specified criteria for spectatorship.</returns>
    public HashSet<ICreature> GetSpectators(Location location, bool multifloor, bool onlyPlayers,
        MinMax rangeX, MinMax rangeY)
    {
        var search = new SpectatorSearch(ref location, multifloor, rangeX.Min, rangeY.Min,
            rangeX.Max, rangeY.Max, onlyPlayers);
        return _world.QuerySpectators(ref search).ToHashSet();
    }

    /// <summary>
    ///     Retrieves a collection of spectators within a specified range of a given location.
    /// </summary>
    /// <param name="location">The central location around which spectators are searched.</param>
    /// <param name="multifloor">A value indicating whether to include creatures on multiple floors.</param>
    /// <param name="onlyPlayers">A value indicating whether to include only player-controlled creatures.</param>
    /// <param name="minRangeX">The minimum range along the X-axis to include spectators.</param>
    /// <param name="maxRangeX">The maximum range along the X-axis to include spectators.</param>
    /// <param name="minRangeY">The minimum range along the Y-axis to include spectators.</param>
    /// <param name="maxRangeY">The maximum range along the Y-axis to include spectators.</param>
    /// <returns>
    ///     Returns a collection of <see cref="ICreature" /> objects representing the spectators within the specified
    ///     range.
    /// </returns>
    public HashSet<ICreature> GetSpectators(Location location, bool multifloor, bool onlyPlayers,
        int minRangeX, int maxRangeX, int minRangeY, int maxRangeY)
    {
        var rangeX = new MinMax(minRangeX, maxRangeX);
        var rangeY = new MinMax(minRangeY, maxRangeY);
        return GetSpectators(location, multifloor, onlyPlayers, rangeX, rangeY);
    }

    /// <summary>
    ///     Retrieves the next tile in the specified direction from the given location.
    /// </summary>
    /// <param name="fromLocation">The starting location from which to determine the next tile.</param>
    /// <param name="direction">The direction in which to locate the next tile.</param>
    /// <returns>Returns the tile located in the specified direction from the given location.</returns>
    public ITile GetNextTile(Location fromLocation, Direction direction)
    {
        var toLocation = fromLocation.GetNextLocation(direction);
        return this[toLocation];
    }

    /// <summary>
    ///     Places a specified creature on the map if the conditions for placement are met.
    /// </summary>
    /// <param name="creature">The creature to be placed on the map.</param>
    public void PlaceCreature(ICreature creature)
    {
        if (this[creature.Location] is not IDynamicTile tile) return;

        if (!tile.CanEnter(creature)) return;
        var creatureAlreadyInTile = false;

        if (tile.HasAnyCreature)
        {
            creatureAlreadyInTile = tile.HasCreature(creature);

            if (!creatureAlreadyInTile)
                foreach (var location in tile.Location.Neighbours)
                    if (this[location] is IDynamicTile { HasAnyCreature: false } t)
                    {
                        tile = t;
                        break;
                    }
        }

        if (_cylinderOperation.AddCreature(creature, tile, out var cylinder).Succeeded is false) return;

        if (!creatureAlreadyInTile)
        {
            var sector = _world.GetSector(creature.Location.X, creature.Location.Y);
            sector.AddCreature(creature);
            creature.Appear(tile.Location, cylinder.TileSpectators);
        }

        if (creature is IWalkableCreature walkableCreature && !creatureAlreadyInTile)
            _eventAggregator.InvokeEvent(new CreatureAddedOnMapEvent(walkableCreature, cylinder));
    }

    /// <summary>
    ///     Removes the specified creature from the map, updating its location, notifying relevant spectators,
    ///     and invoking any necessary events for walkable creatures.
    /// </summary>
    /// <param name="creature">The creature instance to be removed from the map.</param>
    public void RemoveCreature(ICreature creature)
    {
        if (this[creature.Location] is not DynamicTile tile) return;

        _cylinderOperation.RemoveCreature(creature, out var cylinder);

        _world.GetSector(tile.Location.X, tile.Location.Y).RemoveCreature(creature);

        //Notify all spectators about the creature's disappearance
        foreach (var cylinderSpectator in cylinder.TileSpectators)
            cylinderSpectator.Spectator.OnCreatureDisappear(creature);

        if (creature is IWalkableCreature walkableCreature)
            _eventAggregator.InvokeEvent(new ThingRemovedFromTileEvent(walkableCreature, cylinder));
    }

    /// <summary>
    ///     Checks if there are any players around the specified location who can see it.
    /// </summary>
    /// <param name="location">The location to check for nearby players.</param>
    /// <returns>Returns true if there are players around the location who can see it; otherwise, false.</returns>
    public bool ArePlayersAround(Location location)
    {
        foreach (var player in GetPlayersAtPositionZone(location))
            if (player.CanSee(location))
                return true;

        return false;
    }

    /// <summary>
    ///     Determines whether the specified creature can move in the given direction based on the provided tile enter rule.
    /// </summary>
    /// <param name="creature">The creature attempting to move.</param>
    /// <param name="direction">The direction in which the creature intends to move.</param>
    /// <param name="rule">The rule used to evaluate whether the creature is allowed to enter the target tile.</param>
    /// <returns>Returns true if the creature is allowed to move to the tile in the specified direction; otherwise, false.</returns>
    public bool CanGoToDirection(ICreature creature, Direction direction, ITileEnterRule rule)
    {
        var tile = GetNextTile(creature.Location, direction);
        return rule.ShouldIgnore(tile, creature);
    }

    public void OnItemReduced(ICumulative item, byte amount)
    {
        if (this[item.Location] is not IDynamicTile tile) return;
        if (item.Amount == 0)
            tile.RemoveItem(item, amount, 0, out var removedThing);
        if (item.Amount > 0)
        {
            tile.TryGetStackPositionOfItem(item, out var stackPosition);
            _eventAggregator.InvokeEvent(new ThingUpdatedOnTileEvent(item,
                _cylinderOperation.Removed(item, stackPosition)));
        }
    }

    /// <summary>
    ///     Retrieves the destination tile of the specified location, considering dynamic tile mechanics.
    /// </summary>
    /// <returns>Returns the destination <see cref="ITile" /> if the input is dynamic; otherwise, returns the original tile.</returns>
    public ITile GetTileDestination(Location location)
    {
        var toTile = this[location];
        if (toTile is not IDynamicTile destination) return toTile;

        return GetTileDestination(destination);
    }
}