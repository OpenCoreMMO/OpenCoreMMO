using System;
using System.Collections.Generic;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;

namespace NeoServer.Game.World.Models;

public class Sector
{
    public const byte MAP_MAX_LAYERS = 16;
    public const byte SECTOR_MASK = Region.SECTOR_SIZE - 1;
    private readonly ITile[,,] Tiles = new ITile[MAP_MAX_LAYERS, Region.SECTOR_SIZE, Region.SECTOR_SIZE];

    public Sector(Sector north, Sector south, Sector west, Sector east)
    {
        UpdateNeighborsSetors(north, south, west, east);
    }

    public Sector South { get; private set; }
    public Sector East { get; private set; }
    public uint Floors { get; private set; }
    public HashSet<ICreature> Creatures { get; } = new();
    public HashSet<ICreature> Players { get; } = new();
    public List<ICreature> SpectatorsCache { get; } = new(32);

    public void AddTile(ITile tile, Location location)
    {
        var z = location.Z;
        var x = location.X;
        var y = location.Y;

        if (z >= MAP_MAX_LAYERS) return;

        CreateFloor(z);

        if (GetTile(location) is not null) return;

        Tiles[z, x & SECTOR_MASK, y & SECTOR_MASK] = tile;
    }
    public void AddTile(ITile tile)
    {
        var z = tile.Location.Z;
        var x = tile.Location.X;
        var y = tile.Location.Y;

        if (z >= MAP_MAX_LAYERS) return;

        CreateFloor(z);

        if (GetTile(tile.Location) is not null) return;

        Tiles[z, x & SECTOR_MASK, y & SECTOR_MASK] = tile;
    }

    public void CreateFloor(byte z)
    {
        Floors |= (uint)(1 << z);
    }

    public void UpdateNeighborsSetors(Sector north, Sector south, Sector west, Sector east)
    {
        if (north is not null) north.South = this;
        if (west is not null) west.East = this;
        if (south is not null) South = south;
        if (east is not null) East = east;
    }

    public ITile GetTile(Location location)
    {
        if (location.Z >= MAP_MAX_LAYERS) return null;

        return Tiles[location.Z, location.X & SECTOR_MASK, location.Y & SECTOR_MASK];
    }

    public void ReplaceTile(ITile newTile)
    {
        var z = newTile.Location.Z;
        var x = newTile.Location.X;
        var y = newTile.Location.Y;

        if (z >= MAP_MAX_LAYERS) return;

        CreateFloor(z);

        Tiles[z, x & SECTOR_MASK, y & SECTOR_MASK] = newTile;
    }

    public void AddCreature(ICreature creature)
    {
        Creatures.Add(creature);
        if (creature is IPlayer player) Players.Add(player);
        SpectatorsCache.Clear();
    }

    public void RemoveCreature(ICreature creature)
    {
        Creatures.Remove(creature);

        if (creature is IPlayer player)
            Players.Remove(player);

        SpectatorsCache.Clear();
    }

    //public HashSet<ICreature> GetSpectators(ref SpectatorSearch search)
    //{
    //    if (search.CenterPosition.Z >= MAP_MAX_LAYERS) return null;

    //}
}

public readonly ref struct SpectatorSearch
{
    public MinMax RangeX { get; }
    public MinMax RangeY { get; }
    public MinMax RangeZ { get; }
    public bool OnlyPlayers { get; }
    public Location CenterPosition { get; }
    public bool Multifloor { get; }

    public SpectatorSearch(ref Location center, bool multifloor, int minRangeX, int maxRangeX, int minRangeY,
        int maxRangeY, bool onlyPlayers)
    {
        Multifloor = multifloor;
        var minY = minRangeY == 0 ? (int)MapViewPort.MaxViewPortY : minRangeY;
        var minX = minRangeX == 0 ? (int)MapViewPort.MaxViewPortX : minRangeX;
        var maxY = maxRangeY == 0 ? (int)MapViewPort.MaxViewPortY : maxRangeY;
        var maxX = maxRangeX == 0 ? (int)MapViewPort.MaxViewPortX : maxRangeX;

        RangeX = new MinMax(minX, maxX);
        RangeY = new MinMax(minY, maxY);
        RangeZ = GetRangeZ(center, multifloor);

        CenterPosition = center;
        OnlyPlayers = onlyPlayers;
    }

    private static MinMax GetRangeZ(Location center, bool multifloor)
    {
        var minRangeZ = center.Z;
        var maxRangeZ = center.Z;
        if (multifloor)
        {
            if (center.IsUnderground)
            {
                minRangeZ = (byte)Math.Max(center.Z - 2, 0);
                maxRangeZ = (byte)Math.Max(center.Z + 2, Sector.MAP_MAX_LAYERS - 1);
            }
            else if (center.Z == 6)
            {
                minRangeZ = 0;
                maxRangeZ = 8;
            }
            else if (center.Z == 7)
            {
                minRangeZ = 0;
                maxRangeZ = 9;
            }
            else
            {
                minRangeZ = 0;
                maxRangeZ = 7;
            }
        }

        return new MinMax(minRangeZ, maxRangeZ);
    }
}