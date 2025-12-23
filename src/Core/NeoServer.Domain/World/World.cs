using System.Collections.Concurrent;
using System.Collections.Immutable;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.World.Models;
using NeoServer.Domain.World.Models.Tiles;

namespace NeoServer.Domain.World;

public class World
{
    private readonly Region _region = new();

    private readonly ConcurrentDictionary<Coordinate, ITown> _towns = new();
    private readonly ConcurrentDictionary<Coordinate, IWaypoint> _waypoints = new();
    public int LoadedTilesCount { get; private set; }
    public int LoadedTownsCount => _towns.Count;
    public int LoadedWaypointsCount => _waypoints.Count;

    public ImmutableList<ISpawn> Spawns { get; private set; }

    public WorldLight WorldLight { get; private set; } = new();

    public void AddTile(ITile newTile, Location location)
    {
        var sector = _region.CreateSector(location.X, location.Y, out _);

        sector.AddTile(newTile, location);
        LoadedTilesCount++;
    }

    public void AddTile(ITile newTile)
    {
        var sector = _region.CreateSector(newTile.Location.X, newTile.Location.Y, out _);

        sector.AddTile(newTile);
        LoadedTilesCount++;
    }

    public void ReplaceTile(ITile newTile)
    {
        var sector = _region.CreateSector(newTile.Location.X, newTile.Location.Y, out _);

        sector.ReplaceTile(newTile);
        LoadedTilesCount++;
    }

    public void LoadSpawns(IEnumerable<ISpawn> spawns)
    {
        if (spawns.IsNull()) return;

        Spawns = spawns.ToImmutableList();
    }

    public bool TryGetTile(ref Location location, out ITile tile)
    {
        tile = null;
        var sector = _region.GetSector(location.X, location.Y);
        if (sector is null) return false;

        tile = sector.GetTile(location);
        if (tile is null) return false;

        if (tile is StaticTile)
            tile.SetNewLocation(location,
                true); //static tiles are cached and have no location, the location will be always the same as the tile's map location

        return true;
    }

    public Sector GetSector(ushort x, ushort y)
    {
        return _region.GetSector(x, y);
    }

    internal IEnumerable<ICreature> QuerySpectators(ref SpectatorSearch search)
    {
        return _region.GetSpectators(ref search);
    }

    public void AddTown(ITown town)
    {
        if (town.IsNull()) return;
        _towns[town.Coordinate] = town;
    }

    public bool TryGetTown(Location location, out ITown town)
    {
        return _towns.TryGetValue(new Coordinate(location.X, location.Y, (sbyte)location.Z), out town);
    }

    public bool TryGetTown(uint id, out ITown town)
    {
        foreach (var item in _towns)
            if (item.Value.Id == id)
            {
                town = item.Value;
                return true;
            }

        town = null;
        return false;
    }

    public bool TryGetTown(string name, out ITown town)
    {
        foreach (var item in _towns)
            if (item.Value.Name == name)
            {
                town = item.Value;
                return true;
            }

        town = null;
        return false;
    }

    public void AddWaypoint(IWaypoint waypoint)
    {
        if (waypoint.IsNull()) return;

        _waypoints[waypoint.Coordinate] = waypoint;
    }

    public bool TryGetWaypoint(Location location, out IWaypoint waypoint)
    {
        return _waypoints.TryGetValue(new Coordinate(location.X, location.Y, (sbyte)location.Z), out waypoint);
    }

    public void SwapCreatureBetweenSectors(ICreature creature, Location fromLocation, Location toLocation)
    {
        var oldSector = GetSector(fromLocation.X, fromLocation.Y);
        var newSector = GetSector(toLocation.X, toLocation.Y);

        if (oldSector != newSector)
        {
            oldSector.RemoveCreature(creature);
            newSector.AddCreature(creature);
        }
    }
}