using System;
using System.Buffers;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;

namespace NeoServer.Game.Common.Effects.Magical;

public class SpreadEffect
{
    /// <summary>
    ///     Creates a spread effect based on length
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="length"></param>
    /// <param name="spread"></param>
    /// <returns></returns>
    public static Coordinate[] Create(Direction direction, int length, int spread)
    {
        var pool = ArrayPool<Coordinate>.Shared;
        var points = pool.Rent(length * spread);

        if (spread == 0) return [];

        var y = 0;
        var x = 0;

        var count = 0;
        for (var i = 0; i < length; i++)
        {
            var row = i + 1;
            var cols = i < length / spread ? 0 : (i + 1) / (length / spread + 1);
            for (var c = 0 - cols; c <= 0 + cols; c++)
                switch (direction)
                {
                    case Direction.North:
                        points[count++] = new Coordinate(x - c, -row, 0);
                        break;
                    case Direction.East:
                        points[count++] = new Coordinate(+row, y + c, 0);
                        break;
                    case Direction.South:
                        points[count++] = new Coordinate(x + c, +row, 0);
                        break;
                    case Direction.West:
                        points[count++] = new Coordinate(-row, y - c, 0);
                        break;
                    case Direction.None:
                        break;
                }
        }

        pool.Return(points);

        return points[..count];
    }

    public static Coordinate[] Create(Location.Structs.Location location, Direction direction, int length,
        int spread = 1)
    {
        var i = 0;

        var affectedLocations = Create(direction, length, spread);
        var affectedArea = new Coordinate[affectedLocations.Length];

        foreach (var affectedLocation in affectedLocations)
            affectedArea[i++] = location.Translate() + affectedLocation;
        return affectedArea;
    }
}