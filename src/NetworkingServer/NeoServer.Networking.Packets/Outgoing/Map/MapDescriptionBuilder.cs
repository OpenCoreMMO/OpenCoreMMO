using System;
using System.Collections.Generic;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;

namespace NeoServer.Networking.Packets.Outgoing.Map;

/// <summary>
/// Provides static methods for building map description byte arrays for the game protocol.
/// This handles the serialization of map tiles into the wire format required by the client.
/// </summary>
public static class MapDescriptionBuilder
{
    /// <summary>
    /// Builds a complete map description starting from the specified coordinates.
    /// Returns the raw bytes that represent all visible tiles from the player's perspective.
    /// </summary>
    /// <param name="map">The map instance to query tiles from.</param>
    /// <param name="thing">The thing (usually player) requesting the description.</param>
    /// <param name="fromX">Starting X coordinate.</param>
    /// <param name="fromY">Starting Y coordinate.</param>
    /// <param name="currentZ">Current Z (floor) level.</param>
    /// <param name="windowSizeX">Width of the viewing window (default 18).</param>
    /// <param name="windowSizeY">Height of the viewing window (default 14).</param>
    /// <returns>List of bytes representing the map description in protocol format.</returns>
    public static IList<byte> GetDescription(IMap map, IThing thing, ushort fromX, ushort fromY, byte currentZ,
        byte windowSizeX = MapConstants.DEFAULT_MAP_WINDOW_SIZE_X,
        byte windowSizeY = MapConstants.DEFAULT_MAP_WINDOW_SIZE_Y)
    {
        var tempBytes = new List<byte>();

        var skip = -1;

        // we crawl from the ground up to the very top of the world (7 -> 0).
        int crawlTo;
        int crawlFrom;
        int crawlDelta;
        // Unless... we're underground.
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
            tempBytes.AddRange(GetFloorDescription(map, thing, fromX, fromY, (byte)nz, windowSizeX, windowSizeY,
                currentZ - nz, ref skip));

        if (skip >= 0)
        {
            tempBytes.Add((byte)skip);
            tempBytes.Add(0xFF);
        }

        return tempBytes;
    }

    /// <summary>
    /// Builds a description for a single floor of the map.
    /// Returns the raw bytes representing all tiles on the specified floor within the viewing area.
    /// </summary>
    /// <param name="map">The map instance to query tiles from.</param>
    /// <param name="thing">The thing (usually player) requesting the description.</param>
    /// <param name="fromX">Starting X coordinate.</param>
    /// <param name="fromY">Starting Y coordinate.</param>
    /// <param name="currentZ">Current Z (floor) level.</param>
    /// <param name="width">Width of the area to describe.</param>
    /// <param name="height">Height of the area to describe.</param>
    /// <param name="verticalOffset">Vertical offset for multi-floor viewing.</param>
    /// <param name="skip">Reference to skip counter for protocol optimization.</param>
    /// <returns>List of bytes representing the floor description in protocol format.</returns>
    public static IList<byte> GetFloorDescription(IMap map, IThing thing, ushort fromX, ushort fromY, byte currentZ, byte width,
        byte height, int verticalOffset, ref int skip)
    {
        var tempBytes = new List<byte>();

        byte start = 0xFE;
        byte end = 0xFF;

        for (var nx = 0; nx < width; nx++)
        for (var ny = 0; ny < height; ny++)
        {
            var tile = map[(ushort)(fromX + nx + verticalOffset), (ushort)(fromY + ny + verticalOffset),
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
}
