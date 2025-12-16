using System.Diagnostics.CodeAnalysis;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.World.Map;

public class CylinderOperation(IMap map)
{
    /// <summary>
    ///     Creates a cylinder instance as removed
    /// </summary>
    /// <param name="thing"></param>
    /// <param name="amount"></param>
    /// <param name="stackPosition"></param>
    /// <returns></returns>
    public Cylinder Removed(IThing thing, byte stackPosition)
    {
        var spectators = map.GetCreaturesAtPositionZone(thing.Location, thing.Location);

        var tile = map[thing.Location];
        var tileSpectators = new ICylinderSpectator[spectators.Count];

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

    public Cylinder Added(IThing thing)
    {
        var tile = map[thing.Location];

        var spectators = map.GetCreaturesAtPositionZone(tile.Location, tile.Location);

        var tileSpectators = new ICylinderSpectator[spectators.Count];
        var index = 0;

        foreach (var spectator in spectators)
        {
            byte stackPosition = 0;
            if (spectator is IPlayer player) tile.TryGetStackPositionOfThing(player, thing, out stackPosition);

            tileSpectators[index++] = new CylinderSpectator(spectator, stackPosition, stackPosition);
        }

        return new Cylinder(thing, tile, tile, Operation.Added, tileSpectators);
    }

    public Cylinder Updated(IThing thing, byte amount)
    {
        var tile = map[thing.Location];

        var spectators = new HashSet<ICylinderSpectator>();
        foreach (var spec in Removed(thing, amount).TileSpectators) spectators.Add(spec);
        foreach (var spec in Added(thing).TileSpectators)
            if (spectators.TryGetValue(spec, out var spectator))
                spectator.ToStackPosition = spec.ToStackPosition;
            else
                spectators.Add(spec);
        return new Cylinder(thing, tile, tile, Operation.Updated, spectators.ToArray());
    }

    public Result<OperationResultList<ICreature>> RemoveCreature(ICreature creature, out ICylinder cylinder)
    {
        cylinder = null;
        if (map[creature.Location] is not DynamicTile tile) return new Result<OperationResultList<ICreature>>();

        var tileSpectators = GetSpectators(creature, tile);

        var result = tile.RemoveCreature(creature, out var removedCreature);
        cylinder = new Cylinder(removedCreature, tile, tile, Operation.Removed, tileSpectators);
        return result;
    }

    public Result<OperationResultList<ICreature>> AddCreature(ICreature creature, IDynamicTile toTile,
        out ICylinder cylinder)
    {
        cylinder = null;
        if (toTile is not DynamicTile tile) return new Result<OperationResultList<ICreature>>();

        var result =
            new Result<OperationResultList<ICreature>>(new OperationResultList<ICreature>(Operation.Added, creature));

        if (!toTile.HasCreature(creature))
        {
            result = tile.AddCreature(creature);

            if (result.Succeeded is false) return result;
        }

        var tileSpectators = GetSpectators(creature, tile);

        cylinder = new Cylinder(creature, tile, tile, Operation.Added, tileSpectators);
        return result;
    }

    private ICylinderSpectator[] GetSpectators(IThing thing, ITile tile)
    {
        var spectators = map.GetCreaturesAtPositionZone(tile.Location, tile.Location);
        return GetSpectatorsStackPositions(thing, tile, spectators);
    }

    private ICylinderSpectator[] GetSpectatorsStackPositions(IThing thing, ITile tile,
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

    public Result<OperationResultList<ICreature>> MoveCreature(ICreature creature, IDynamicTile fromTile,
        IDynamicTile toTile, byte amount, bool forced, out ICylinder cylinder)
    {
        amount = amount == 0 ? (byte)1 : amount;

        cylinder = null;

        var specs = map.GetSpectators(fromTile.Location, toTile.Location);
        var spectators = GetSpectatorsStackPositions(creature, fromTile, specs);
        var result = ((DynamicTile)fromTile).RemoveCreature(creature, out _);

        if (result.Succeeded is false) return result;

        map.SwapCreatureBetweenSectors(creature, fromTile.Location, toTile.Location);

        var result2 = ((DynamicTile)toTile).AddCreature(creature, forced: forced);

        cylinder = new Cylinder(creature, fromTile, toTile, Operation.Moved, spectators.ToArray());
        return result2;
    }
}

public class CylinderSpectator(ICreature spectator, byte fromStackPosition, byte toStackPosition)
    : IEqualityComparer<ICylinderSpectator>, ICylinderSpectator
{
    public byte FromStackPosition { get; set; } = fromStackPosition;
    public byte ToStackPosition { get; set; } = toStackPosition;
    public ICreature Spectator { get; } = spectator;

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
    public bool IsTeleport => !ToTile.Location.IsNotInRange(FromTile.Location, 1, 1, 0);
}