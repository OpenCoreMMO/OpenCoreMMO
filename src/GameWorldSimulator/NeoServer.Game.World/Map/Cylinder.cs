using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Results;
using NeoServer.Game.World.Models.Tiles;

namespace NeoServer.Game.World.Map;

public class CylinderOperation
{
    private static IMap _map;

    public static void Setup(IMap map)
    {
        _map = map;
    }

    /// <summary>
    ///     Creates a cylinder instance as removed
    /// </summary>
    /// <param name="thing"></param>
    /// <param name="amount"></param>
    /// <param name="stackPosition"></param>
    /// <returns></returns>
    public static Cylinder Removed(IThing thing, byte stackPosition)
    {
        var spectators = _map.GetCreaturesAtPositionZone(thing.Location, thing.Location);

        var tile = _map[thing.Location];
        var tileSpectators = new ICylinderSpectator[spectators.Count()];

        var index = 0;
        foreach (var spectator in spectators)
        {
            var fromStackPosition = stackPosition;

            if (spectator is IPlayer player)
                if (thing is IItem { IsAlwaysOnTop: false } and not IGround)
                    fromStackPosition = (byte)(tile.GetCreatureStackPositionIndex(player) + stackPosition);

            tileSpectators[index++] = new CylinderSpectator(spectator, fromStackPosition, fromStackPosition);
        }

        return new Cylinder(thing, tile, tile, Operation.Removed, tileSpectators);
    }

    public static Cylinder Added(IThing thing)
    {
        var tile = _map[thing.Location];

        var spectators = _map.GetCreaturesAtPositionZone(tile.Location, tile.Location);

        var tileSpectators = new ICylinderSpectator[spectators.Count];
        var index = 0;

        foreach (var spectator in spectators)
        {
            byte stackPosition = default;
            if (spectator is IPlayer player) tile.TryGetStackPositionOfThing(player, thing, out stackPosition);

            tileSpectators[index++] = new CylinderSpectator(spectator, stackPosition, stackPosition);
        }

        return new Cylinder(thing, tile, tile, Operation.Added, tileSpectators);
    }

    public static Cylinder Updated(IThing thing, byte amount)
    {
        var tile = _map[thing.Location];

        var spectators = new HashSet<ICylinderSpectator>();
        foreach (var spec in Removed(thing, amount).TileSpectators) spectators.Add(spec);
        foreach (var spec in Added(thing).TileSpectators)
            if (spectators.TryGetValue(spec, out var spectator))
                spectator.ToStackPosition = spec.ToStackPosition;
            else
                spectators.Add(spec);
        return new Cylinder(thing, tile, tile, Operation.Updated, spectators.ToArray());
    }

    public static Result<OperationResultList<ICreature>> RemoveCreature(ICreature creature, out ICylinder cylinder)
    {
        cylinder = null;
        if (_map[creature.Location] is not DynamicTile tile) return new Result<OperationResultList<ICreature>>();

        var tileSpectators = GetSpectators(creature, tile);

        var result = tile.RemoveCreature(creature, out var removedCreature);
        cylinder = new Cylinder(removedCreature, tile, tile, Operation.Removed, tileSpectators);
        return result;
    }

    public static Result<OperationResultList<ICreature>> AddCreature(ICreature creature, IDynamicTile toTile,
        out ICylinder cylinder)
    {
        cylinder = null;
        if (toTile is not DynamicTile tile) return new Result<OperationResultList<ICreature>>();

        var result = tile.AddCreature(creature);

        if (result.Succeeded is false) return result;

        var tileSpectators = GetSpectators(creature, tile);

        cylinder = new Cylinder(creature, tile, tile, Operation.Added, tileSpectators);
        return result;
    }

    private static ICylinderSpectator[] GetSpectators(IThing thing, ITile tile)
    {
        var spectators = _map.GetCreaturesAtPositionZone(tile.Location, tile.Location);
        return GetSpectatorsStackPositions(thing, tile, spectators);
    }

    private static ICylinderSpectator[] GetSpectatorsStackPositions(IThing thing, ITile tile,
        HashSet<ICreature> spectators)
    {
        var tileSpectators = new ICylinderSpectator[spectators.Count];
        var index = 0;

        foreach (var spectator in spectators)
        {
            byte stackPosition = default;
            if (spectator is IPlayer player) tile.TryGetStackPositionOfThing(player, thing, out stackPosition);

            tileSpectators[index++] = new CylinderSpectator(spectator, stackPosition, 0xFF);
        }

        return tileSpectators;
    }

    public static Result<OperationResultList<ICreature>> MoveCreature(ICreature creature, IDynamicTile fromTile,
        IDynamicTile toTile, byte amount, out ICylinder cylinder)
    {
        amount = amount == 0 ? (byte)1 : amount;

        cylinder = null;

        var specs = _map.GetSpectators(fromTile.Location, toTile.Location);
        var spectators = GetSpectatorsStackPositions(creature, fromTile, specs);
        var result = ((DynamicTile)fromTile).RemoveCreature(creature, out _);

        if (result.Succeeded is false) return result;

        _map.SwapCreatureBetweenSectors(creature, fromTile.Location, toTile.Location);

        var result2 = ((DynamicTile)toTile).AddCreature(creature);

        cylinder = new Cylinder(creature, fromTile, toTile, Operation.Moved, spectators.ToArray());
        return result2;
    }
}

public class CylinderSpectator : IEqualityComparer<ICylinderSpectator>, ICylinderSpectator
{
    public CylinderSpectator(ICreature spectator, byte fromStackPosition, byte toStackPosition)
    {
        FromStackPosition = fromStackPosition;
        ToStackPosition = toStackPosition;
        Spectator = spectator;
    }

    public byte FromStackPosition { get; set; }
    public byte ToStackPosition { get; set; }
    public ICreature Spectator { get; }

    public bool Equals(ICylinderSpectator x, ICylinderSpectator y)
    {
        return x.Spectator == y.Spectator;
    }

    public int GetHashCode([DisallowNull] ICylinderSpectator obj)
    {
        return HashCode.Combine(obj.Spectator.CreatureId);
    }

    public override bool Equals(object obj)
    {
        return obj is ICylinderSpectator spec && Spectator == spec.Spectator;
    }

    public override int GetHashCode()
    {
        return GetHashCode(this);
    }
}

public record Cylinder(
    IThing Thing,
    ITile FromTile,
    ITile ToTile,
    Operation Operation,
    ICylinderSpectator[] TileSpectators) : ICylinder
{
    public bool IsTeleport => ToTile.Location.GetMaxSqmDistance(FromTile.Location) > 1;
}