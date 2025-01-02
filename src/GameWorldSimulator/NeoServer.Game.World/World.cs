using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.World.Models;

namespace NeoServer.Game.World;

public class World
{
    private readonly Region region = new();

    private readonly ConcurrentDictionary<Coordinate, ITown> towns = new();
    private readonly ConcurrentDictionary<Coordinate, IWaypoint> waypoints = new();
    public int LoadedTilesCount { get; private set; }
    public int LoadedTownsCount => towns.Count();
    public int LoadedWaypointsCount => waypoints.Count();

    public ImmutableList<ISpawn> Spawns { get; private set; }

    
    public void AddTile(ITile newTile, Location location)
    {
        var sector = region.CreateSector(location.X, location.Y, out var created);

        sector.AddTile(newTile, location);
        LoadedTilesCount++;
    }

    public void AddTile(ITile newTile)
    {
        var sector = region.CreateSector(newTile.Location.X, newTile.Location.Y, out var created);

        sector.AddTile(newTile);
        LoadedTilesCount++;
    }

    public void ReplaceTile(ITile newTile)
    {
        var x = newTile.Location.X;
        var y = newTile.Location.Y;

        var sector = region.CreateSector(newTile.Location.X, newTile.Location.Y, out var created);

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
        var sector = region.GetSector(location.X, location.Y);
        if (sector is null) return false;

        tile = sector.GetTile(location);
        if (tile is null) return false;
        return true;
    }

    public Sector GetSector(ushort x, ushort y)
    {
        return region.GetSector(x, y);
    }

    public IEnumerable<ICreature> GetSpectators(ref SpectatorSearch search)
    {
        return region.GetSpectators(ref search);
    }

    public void AddTown(ITown town)
    {
        if (town.IsNull()) return;
        towns[town.Coordinate] = town;
    }

    public bool TryGetTown(Location location, out ITown town)
    {
        return towns.TryGetValue(new Coordinate(location.X, location.Y, (sbyte)location.Z), out town);
    }

    public bool TryGetTown(uint id, out ITown town)
    {
        foreach (var item in towns)
            if (item.Value.Id == id)
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

        waypoints[waypoint.Coordinate] = waypoint;
    }

    public bool TryGetWaypoint(Location location, IWaypoint waypoint)
    {
        return waypoints.TryGetValue(new Coordinate(location.X, location.Y, (sbyte)location.Z), out waypoint);
    }
}