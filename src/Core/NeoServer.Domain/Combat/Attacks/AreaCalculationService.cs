using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.World.Algorithms;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.Combat.Attacks;

public class AffectedTargets
{
    public List<Location> Locations { get; }
    public List<ICreature> Creatures { get; }

    public AffectedTargets(List<Location> locations, List<ICreature> creatures)
    {
        Locations = locations;
        Creatures = creatures;
    }
}

public class AreaCalculationService(IMap map)
{
    public AffectedTargets CalculateAffectedTargets(Location originLocation, Coordinate[] area)
    {
        var affectedArea = new List<Location>(area.Length);
        var affectedCreatures = new List<ICreature>();

        foreach (var coordinate in area)
        {
            var location = coordinate.Location;
            var tile = map[location] ?? new EmptyTile(location);

            // Check if the tile is walkable and clear of obstacles
            if (tile.ProtectionZone || tile.BlockMissile || tile is IDynamicTile { HasHole: true })
            {
                continue;
            }

            // Check if the line of sight is clear between aggressor and target location
            if (!SightClear.IsSightClear(map, originLocation, tile.Location, false)) continue;

            affectedArea.Add(location);

            if (tile is not IDynamicTile targetTile)
            {
                continue;
            }

            var targetCreatures = targetTile.Creatures?.ToArray();
            if (targetCreatures is null or { Length: 0 }) continue;

            affectedCreatures.AddRange(targetCreatures);
        }

        return new AffectedTargets(affectedArea, affectedCreatures);
    }
}