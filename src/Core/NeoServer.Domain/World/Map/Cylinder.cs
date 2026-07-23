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
    /// Creates a cylinder instance with the operation status set to removed.
    /// </summary>
    /// <param name="thing">The item or entity that is being removed.</param>
    /// <param name="stackPosition">The stack position of the item or entity being removed.</param>
    /// <returns>A new instance of <see cref="Cylinder" /> representing the removed operation.</returns>
    public Cylinder Removed(IThing thing, byte stackPosition)
    {
        var spectators = map.GetCreaturesAtPositionZone(thing.Location, thing.Location);

        var tile = map[thing.Location];
        var tileSpectators = new ICylinderSpectator[spectators.Count];

        var index = 0;
        foreach (var spectator in spectators)
        {
            var fromStackPosition = stackPosition;

            if (spectator is IPlayer player && thing is IItem { IsAlwaysOnTop: false } and not IGround)
            {
                fromStackPosition = (byte)(tile.GetCreatureStackPositionIndex(player) + stackPosition);
            }

            tileSpectators[index++] = new CylinderSpectator(spectator, fromStackPosition, fromStackPosition);
        }

        return new Cylinder(thing, tile, tile, Operation.Removed, tileSpectators);
    }

    public Cylinder Added(IThing thing)
    {
        var tile = map[thing.Location];

        if (tile is null)
        {
            return new Cylinder(thing, null, null, Operation.None, []);
        }

        var spectators = map.GetCreaturesAtPositionZone(tile.Location, tile.Location);

        var tileSpectators = new ICylinderSpectator[spectators.Count];
        var index = 0;

        foreach (var spectator in spectators)
        {
            byte stackPosition = 0;
            if (spectator is IPlayer player)
            {
                tile.TryGetStackPositionOfThing(player, thing, out stackPosition);
            }

            tileSpectators[index++] = new CylinderSpectator(spectator, stackPosition, stackPosition);
        }

        return new Cylinder(thing, tile, tile, Operation.Added, tileSpectators);
    }

    public Cylinder Updated(IThing thing, byte stackPosition)
    {
        var tile = map[thing.Location];

        var spectators = new HashSet<ICylinderSpectator>();
        
        foreach (var spectator in Removed(thing, stackPosition).TileSpectators)
        {
            spectators.Add(spectator);
        }

        foreach (var cylinderSpectator in Added(thing).TileSpectators)
        {
            if (spectators.TryGetValue(cylinderSpectator, out var spectator))
            {
                spectator.ToStackPosition = cylinderSpectator.ToStackPosition;
            }
            else
            {
                spectators.Add(cylinderSpectator);
            }
        }

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

    private ICylinderSpectator[] GetSpectators(IThing thing, DynamicTile tile)
    {
        var spectators = map.GetCreaturesAtPositionZone(tile.Location, tile.Location);
        return GetSpectatorsStackPositions(thing, tile, spectators);
    }

    private static ICylinderSpectator[] GetSpectatorsStackPositions(IThing thing, DynamicTile tile,
        HashSet<ICreature> spectators)
    {
        var tileSpectators = new ICylinderSpectator[spectators.Count];
        var index = 0;

        foreach (var spectator in spectators)
        {
            byte stackPosition = 0;
            if (spectator is IPlayer player)
            {
                tile.TryGetStackPositionOfThing(player, thing, out stackPosition);
            }

            tileSpectators[index++] = new CylinderSpectator(spectator, stackPosition, 0xFF);
        }

        return tileSpectators;
    }

    public Result<OperationResultList<ICreature>> MoveCreature(ICreature creature, DynamicTile fromTile,
        IDynamicTile toTile, byte amount, bool forced, out ICylinder cylinder)
    {
        amount = amount == 0 ? (byte)1 : amount;

        cylinder = null;

        var specs = map.GetSpectators(fromTile.Location, toTile.Location);
        var spectators = GetSpectatorsStackPositions(creature, fromTile, specs);
        var result = fromTile.RemoveCreature(creature, out _);

        if (!result.Succeeded) return result;

        map.SwapCreatureBetweenSectors(creature, fromTile.Location, toTile.Location);

        var result2 = ((DynamicTile)toTile).AddCreature(creature, forced);

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
        return Equals(x?.Spectator, y?.Spectator);
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