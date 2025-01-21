using System;
using System.Collections.Generic;
using System.Linq;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Results;
using NeoServer.Game.Common.Services;
using NeoServer.Game.Common.Texts;
using NeoServer.Game.World.Algorithms;
using NeoServer.Game.World.Models;
using NeoServer.Game.World.Models.Tiles;

namespace NeoServer.Game.World.Map;

public class Map : IMap
{
    private const int MAP_MAX_LAYERS = 16;
    private readonly World world;

    public Map(World world)
    {
        this.world = world;
        CylinderOperation.Setup(this);
        TileOperationEvent.OnTileChanged += OnTileChanged;
        TileOperationEvent.OnTileLoaded += OnTileLoaded;

        Instance = this;
    }

    public static IMap Instance { get; private set; }

    public event PlaceCreatureOnMap OnCreatureAddedOnMap;
    public event RemoveThingFromTile OnThingRemovedFromTile;
    public event AddThingToTile OnThingAddedToTile;
    public event UpdateThingOnTile OnThingUpdatedOnTile;
    public event MoveCreatureOnFloor OnCreatureMoved;
    public event FailedMoveThing OnThingMovedFailed;

    public ITile this[Location location] => world.TryGetTile(ref location, out var tile) ? tile : null;
    public ITile this[ushort x, ushort y, byte z] => this[new Location(x, y, z)];

    public ITile GetTile(Location location)
    {
        return this[location];
    }

    public void ReplaceTile(ITile newTile)
    {
        world.ReplaceTile(newTile);
    }

    public bool TryMoveCreature(ICreature creature, Location toLocation)
    {
        if (creature is not IWalkableCreature walkableCreature) return false;

        if (this[creature.Location] is not IDynamicTile fromTile)
        {
            OnThingMovedFailed?.Invoke(creature, InvalidOperation.NotPossible);
            return false;
        }

        var tileDestination = this[toLocation];

        if (tileDestination is not IDynamicTile toTile) //immutable tiles cannot be modified
        {
            OnThingMovedFailed?.Invoke(creature, InvalidOperation.NotEnoughRoom);
            return false;
        }

        var result = CylinderOperation.MoveCreature(creature, fromTile, toTile, 1, out var cylinder);
        if (result.Succeeded is false) return false;

        walkableCreature.OnMoved(fromTile, toTile, cylinder.TileSpectators);
        OnCreatureMoved?.Invoke(walkableCreature, cylinder);

        if (toTile.HasTeleport(out var teleport) && teleport.HasDestination)
        {
            teleport.Teleport(walkableCreature);
            return true;
        }

        tileDestination = GetTileDestination(tileDestination);

        if (tileDestination is null || tileDestination.Location == toLocation) return true;

        TryMoveCreature(creature, tileDestination.Location);

        return true;
    }

    public void SwapCreatureBetweenSectors(ICreature creature, Location fromLocation, Location toLocation)
    {
        var oldSector = world.GetSector(fromLocation.X, fromLocation.Y);
        var newSector = world.GetSector(toLocation.X, toLocation.Y);

        if (oldSector != newSector)
        {
            oldSector.RemoveCreature(creature);
            newSector.AddCreature(creature);
        }
    }

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

    public HashSet<ICreature> GetSpectators(Location fromLocation, bool onlyPlayers = false)
    {
        return GetSpectators(fromLocation, fromLocation, onlyPlayers);
    }

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
            return world.GetSpectators(ref search).ToHashSet();
        }

        var oldSpecs = GetSpectators(fromLocation);
        var newSpecs = GetSpectators(toLocation);
        oldSpecs.UnionWith(newSpecs);

        return oldSpecs;
    }

    public IEnumerable<ICreature> GetPlayersAtPositionZone(Location location)
    {
        return GetCreaturesAtPositionZone(location, true);
    }

    public HashSet<ICreature> GetCreaturesAtPositionZone(Location location, Location toLocation)
    {
        if (location == toLocation) return GetCreaturesAtPositionZone(location).ToHashSet();

        var fromSpectators = GetCreaturesAtPositionZone(location);
        var toSpectators = GetCreaturesAtPositionZone(toLocation);

        var spectators = new List<ICreature>(fromSpectators.Count() + toSpectators.Count());

        spectators.AddRange(fromSpectators);
        spectators.AddRange(toSpectators);
        return spectators.ToHashSet();
    }

    public IEnumerable<ICreature> GetCreaturesAtPositionZone(Location location, bool onlyPlayers = false)
    {
        return GetSpectators(location, onlyPlayers);
    }

    public IList<byte> GetDescription(IThing thing, ushort fromX, ushort fromY, byte currentZ,
        byte windowSizeX = MapConstants.DEFAULT_MAP_WINDOW_SIZE_X,
        byte windowSizeY = MapConstants.DEFAULT_MAP_WINDOW_SIZE_Y)
    {
        var tempBytes = new List<byte>();

        var skip = -1;

        // we crawl from the ground up to the very top of the world (7 -> 0).
        int crawlTo;
        int crawlFrom;
        int crawlDelta;
        // Unless... we're undeground.
        // Then we crawl from 2 floors up, this, and 2 floors down for a total of 5 floors.
        if (currentZ > 7) //isUnderground
        {
            crawlDelta = 1;
            crawlFrom = currentZ - 2;
            crawlTo = Math.Min(15, currentZ + 2);
        }
        else
        {
            crawlFrom = 7;
            crawlTo = 0;
            crawlDelta = -1;
        }

        for (var nz = crawlFrom; nz != crawlTo + crawlDelta; nz += crawlDelta)
            tempBytes.AddRange(GetFloorDescription(thing, fromX, fromY, (byte)nz, windowSizeX, windowSizeY,
                currentZ - nz, ref skip));

        if (skip >= 0)
        {
            tempBytes.Add((byte)skip);
            tempBytes.Add(0xFF);
        }

        return tempBytes;
    }

    public IList<byte> GetFloorDescription(IThing thing, ushort fromX, ushort fromY, byte currentZ, byte width,
        byte height, int verticalOffset, ref int skip)
    {
        var tempBytes = new List<byte>();

        byte start = 0xFE;
        byte end = 0xFF;

        for (var nx = 0; nx < width; nx++)
        for (var ny = 0; ny < height; ny++)
        {
            var tile = this[(ushort)(fromX + nx + verticalOffset), (ushort)(fromY + ny + verticalOffset),
                currentZ];

            if (tile != null)
            {
                if (skip >= 0)
                {
                    tempBytes.Add((byte)skip);
                    tempBytes.Add(end);
                }

                skip = 0;

                if (tile is IStaticTile immutableTile)
                    tempBytes.AddRange(immutableTile.Raw);
                else if (tile is IDynamicTile mutableTile) tempBytes.AddRange(mutableTile.GetRaw(thing as IPlayer));
            }
            else if (skip == start)
            {
                tempBytes.Add(end);
                tempBytes.Add(end);
                skip = -1;
            }
            else
            {
                ++skip;
            }
        }

        return tempBytes;
    }

    public ITile GetNextTile(Location fromLocation, Direction direction)
    {
        var toLocation = fromLocation.GetNextLocation(direction);
        return this[toLocation];
    }

    public void PlaceCreature(ICreature creature)
    {
        if (this[creature.Location] is not IDynamicTile tile) return;

        if (!tile.CanEnter(creature)) return;

        if (tile.HasCreature)
            foreach (var location in tile.Location.Neighbours)
                if (this[location] is IDynamicTile { HasCreature: false } t
                    && !t.HasFlag(TileFlags.BLockSolid))
                {
                    tile = t;
                    break;
                }

        if (CylinderOperation.AddCreature(creature, tile, out var cylinder).Succeeded is false) return;

        var sector = world.GetSector(creature.Location.X, creature.Location.Y);
        sector.AddCreature(creature);

        creature.OnAppear(tile.Location, cylinder.TileSpectators);
        if (creature is IWalkableCreature walkableCreature)
            OnCreatureAddedOnMap?.Invoke(walkableCreature, cylinder);
    }

    public void RemoveCreature(ICreature creature)
    {
        if (this[creature.Location] is not DynamicTile tile) return;

        CylinderOperation.RemoveCreature(creature, out var cylinder);

        world.GetSector(tile.Location.X, tile.Location.Y).RemoveCreature(creature);
        if (creature is IWalkableCreature walkableCreature)
            OnThingRemovedFromTile?.Invoke(walkableCreature, cylinder);
    }

    public bool ArePlayersAround(Location location)
    {
        foreach (var player in GetPlayersAtPositionZone(location))
            if (player.CanSee(location))
                return true;
        return false;
    }

    public void PropagateAttack(ICombatActor actor, CombatDamage damage, AffectedLocation[] area)
    {
        foreach (var coordinate in area)
        {
            var location = coordinate.Point.Location;
            var tile = this[location];

            if (tile is not IDynamicTile walkableTile || walkableTile.HasFlag(TileFlags.BLockSolid) ||
                walkableTile.ProtectionZone)
            {
                coordinate.MarkAsMissed();
                continue;
            }

            if (!SightClear.IsSightClear(this, actor.Location, location, false))
            {
                coordinate.MarkAsMissed();
                continue;
            }

            var targetCreatures = walkableTile.Creatures?.ToArray();

            if (targetCreatures is null) continue;

            foreach (var target in targetCreatures)
            {
                if (actor == target) continue;

                if (target is not ICombatActor targetCreature)
                {
                    coordinate.MarkAsMissed();
                    continue;
                }

                targetCreature.ReceiveAttack(actor, damage);
            }
        }
    }

    public void MoveCreature(IWalkableCreature creature)
    {
        var nextDirection = creature.GetNextStep();
        if (nextDirection == Direction.None) return;

        var nextTile = GetNextTile(creature.Location, nextDirection);

        if (creature.Location.Z != 8 && creature.Tile.HasHeight(3))
        {
            var toLocation = creature.Location.GetNextLocation(nextDirection);
            var newDestination = new Location(toLocation.X, toLocation.Y, (byte)(toLocation.Z - 1));

            if (this[newDestination] is IDynamicTile newDestinationTile) nextTile = newDestinationTile;
        }

        if (!creature.Location.IsSurface && nextTile is null)
        {
            var toLocation = creature.Location.GetNextLocation(nextDirection);
            var newDestination = toLocation.AddFloors(1);

            if (this[newDestination] is IDynamicTile newDestinationTile && newDestinationTile.HasHeight(3))
                nextTile = newDestinationTile;
        }

        if (nextTile is IDynamicTile dynamicTile && !(dynamicTile.CanEnterFunction?.Invoke(creature) ?? true))
        {
            creature.CancelWalk();
            OperationFailService.Send(creature.CreatureId, TextConstants.NOT_POSSIBLE);
            return;
        }

        if (creature.TileEnterRule.CanEnter(nextTile, creature) && TryMoveCreature(creature, nextTile.Location)) return;

        creature.CancelWalk();
        OperationFailService.Send(creature.CreatureId, TextConstants.NOT_POSSIBLE);
    }

    public void CreateBloodPool(ILiquid pool, IDynamicTile tile)
    {
        //if (tile?.TopItems != null && tile.TopItems.TryPeek(out var topItem) && topItem is ILiquid)
        //{
        //    tile.RemoveItem(topItem, 1, 0, out var removedThing);
        //}

        //if (pool is null) return;
        //tile.AddItem(pool);
    }

    public bool CanGoToDirection(ICreature creature, Direction direction, ITileEnterRule rule)
    {
        var tile = GetNextTile(creature.Location, direction);
        return rule.ShouldIgnore(tile, creature);
    }

    public ITile GetFinalTile(ITile toTile)
    {
        if (toTile is not IDynamicTile destination) return toTile;

        if (destination.HasHole) return GetFinalTile(this[destination.Location.AddFloors(1)]);

        return toTile;
    }

    private void OnTileChanged(ITile tile, IItem item, OperationResultList<IItem> resultList)
    {
        if (!resultList.HasAnyOperation) return;

        foreach (var operation in resultList.Operations)
            switch (operation.Item2)
            {
                case Operation.Removed:
                    if (operation.Item1 is ICumulative cumulativeToRemove) cumulativeToRemove.OnReduced -= OnItemReduced;
                    OnThingRemovedFromTile?.Invoke(operation.Item1,
                        CylinderOperation.Removed(operation.Item1, operation.Item3));
                    break;
                case Operation.Updated:
                    if (operation.Item1 is ICumulative cumulativeToUpdate) cumulativeToUpdate.OnReduced += OnItemReduced;
                    OnThingUpdatedOnTile?.Invoke(operation.Item1,
                        CylinderOperation.Updated(operation.Item1, operation.Item1.Amount));
                    break;
                case Operation.Added:
                    if (operation.Item1 is ICumulative cumulativeToAdd) cumulativeToAdd.OnReduced += OnItemReduced;
                    OnThingAddedToTile?.Invoke(operation.Item1, CylinderOperation.Added(operation.Item1));
                    break;
            }
    }

    private void OnTileLoaded(ITile tile)
    {
        if (tile is not IDynamicTile dynamicTile) return;
        foreach (var item in dynamicTile.AllItems)
            if (item is ICumulative cumulative)
                cumulative.OnReduced += OnItemReduced;
    }

    private void OnItemReduced(ICumulative item, byte amount)
    {
        if (this[item.Location] is not IDynamicTile tile) return;
        if (item.Amount == 0)
            tile.RemoveItem(item, amount, 0, out var removedThing);
        if (item.Amount > 0)
        {
            tile.TryGetStackPositionOfItem(item, out var stackPosition);
            OnThingUpdatedOnTile?.Invoke(item, CylinderOperation.Removed(item, stackPosition));
        }
    }
}