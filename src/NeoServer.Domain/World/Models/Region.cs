using System.Collections.Concurrent;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Location;

namespace NeoServer.Domain.World.Models;

public class Region
{
    public const byte SECTOR_SIZE = 16;

    private readonly ConcurrentDictionary<uint, Sector> Sectors = new();

    /// <summary>
    ///     Creates a new sector or return if it already exists
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public Sector CreateSector(uint x, uint y, out bool created)
    {
        created = false;

        var index = (x / SECTOR_SIZE) | ((y / SECTOR_SIZE) << 16);
        if (Sectors.TryGetValue(index, out var sector)) return sector;

        var north = GetSector(x, (ushort)(y - SECTOR_SIZE));
        var west = GetSector((ushort)(x - SECTOR_SIZE), y);
        var south = GetSector(x, (ushort)(y + SECTOR_SIZE));
        var east = GetSector((ushort)(x + SECTOR_SIZE), y);

        var newSector = new Sector(north, south, west, east);
        Sectors.TryAdd(index, newSector);

        created = true;
        return newSector;
    }

    public Sector GetSector(uint x, uint y)
    {
        var index = (x / SECTOR_SIZE) | ((y / SECTOR_SIZE) << 16);
        Sectors.TryGetValue(index, out var sector);
        return sector;
    }

    public IEnumerable<ICreature> GetSpectators(ref SpectatorSearch search)
    {
        if (search.CenterPosition.Z >= Sector.MAP_MAX_LAYERS) return null;

        var spectators = new List<ICreature>();

        var minRangeX = search.RangeX.Min;
        var maxRangeX = search.RangeX.Max;
        var minRangeY = search.RangeY.Min;
        var maxRangeY = search.RangeY.Max;

        var minRangeZ = search.RangeZ.Min;
        var maxRangeZ = search.RangeZ.Max;

        if (minRangeX == (int)MapViewPort.MaxViewPortX && maxRangeX == (int)MapViewPort.MaxViewPortX &&
            minRangeY == (int)MapViewPort.MaxViewPortY && maxRangeY == (int)MapViewPort.MaxViewPortY &&
            search.Multifloor)
        {
        }

        var centerPos = search.CenterPosition;

        var min_y = centerPos.Y - minRangeY;
        var min_x = centerPos.X - minRangeX;
        var max_y = centerPos.Y + maxRangeY;
        var max_x = centerPos.X + maxRangeX;

        var width = (uint)(max_x - min_x);
        var height = (uint)(max_y - min_y);
        var depth = (uint)(maxRangeZ - minRangeZ);

        var minoffset = centerPos.Z - maxRangeZ;
        var x1 = Math.Min(0xFFFF, Math.Max(0, min_x + minoffset));
        var y1 = Math.Min(0xFFFF, Math.Max(0, min_y + minoffset));

        var maxoffset = centerPos.Z - minRangeZ;
        var x2 = Math.Min(0xFFFF, Math.Max(0, max_x + maxoffset));
        var y2 = Math.Min(0xFFFF, Math.Max(0, max_y + maxoffset));

        var startx1 = x1 - (x1 & Sector.SECTOR_MASK);
        var starty1 = y1 - (y1 & Sector.SECTOR_MASK);
        var endx2 = x2 - (x2 & Sector.SECTOR_MASK);
        var endy2 = y2 - (y2 & Sector.SECTOR_MASK);

        var startSector = GetSector((uint)startx1, (uint)starty1);
        var sectorS = startSector;
        Sector sectorE;
        for (var ny = starty1; ny <= endy2; ny += SECTOR_SIZE)
        {
            sectorE = sectorS;
            for (var nx = startx1; nx <= endx2; nx += SECTOR_SIZE)
                if (sectorE is not null)
                {
                    IEnumerable<ICreature> spectatorsList =
                        search.OnlyPlayers ? sectorE.Players : sectorE.Creatures;
                    foreach (var spec in spectatorsList)
                    {
                        var creature = spec;

                        var creaturePosition = creature.Location;
                        if ((uint)(creaturePosition.Z - minRangeZ) <= depth)
                        {
                            var offsetZ = centerPos.GetOffSetZ(creaturePosition);
                            if ((uint)(creaturePosition.X - offsetZ - min_x) <= width &&
                                (uint)(creaturePosition.Y - offsetZ - min_y) <= height) spectators.Add(creature);
                        }
                    }

                    sectorE = sectorE.East;
                }
                else
                {
                    sectorE = GetSector((ushort)(nx + SECTOR_SIZE), (ushort)ny);
                }

            if (sectorS is not null)
                sectorS = sectorS.South;
            else
                sectorS = GetSector((ushort)startx1, (ushort)(ny + SECTOR_SIZE));
        }

        return spectators;
    }
}