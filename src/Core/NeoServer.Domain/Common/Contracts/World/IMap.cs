using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
namespace NeoServer.Domain.Common.Contracts.World;

public interface IMap
{
    ITile this[Location.Structs.Location location] { get; }
    ITile this[ushort x, ushort y, byte z] { get; }

    bool ArePlayersAround(Location.Structs.Location location);
    void PlaceCreature(ICreature creature);
    ITile GetNextTile(Location.Structs.Location fromLocation, Direction direction);

    HashSet<ICreature> GetPlayersAtPositionZone(Location.Structs.Location location);

    bool IsInRange(Location.Structs.Location start, Location.Structs.Location current, Location.Structs.Location target,
        FindPathParams fpp);

    HashSet<ICreature> GetCreaturesAtPositionZone(Location.Structs.Location location,
        Location.Structs.Location toLocation);

    ITile GetTileDestination(ITile tile);
    void RemoveCreature(ICreature creature);

    void SwapCreatureBetweenSectors(ICreature creature, Location.Structs.Location fromLocation,
        Location.Structs.Location toLocation);

    HashSet<ICreature> GetSpectators(Location.Structs.Location fromLocation, Location.Structs.Location toLocation,
        bool onlyPlayers = false);

    HashSet<ICreature> GetSpectators(Location.Structs.Location fromLocation, bool onlyPlayers = false);

    HashSet<ICreature> GetSpectators(Location.Structs.Location location, bool multifloor, bool onlyPlayers,
        MinMax rangeX, MinMax rangeY);

    HashSet<ICreature> GetSpectators(Location.Structs.Location location, bool multifloor, bool onlyPlayers,
        int minRangeX, int maxRangeX, int minRangeY, int maxRangeY);

    HashSet<ICreature> GetCreaturesAtPositionZone(Location.Structs.Location location, bool onlyPlayers = false);
    bool CanGoToDirection(ICreature creature, Direction direction, ITileEnterRule rule);
    ITile GetTile(Location.Structs.Location location);
}