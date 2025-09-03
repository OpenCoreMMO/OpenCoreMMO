using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Common.Effects.Magical;

public static class SpreadEffect
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
        spread = Math.Max(spread, 1);
        var maxCols = (length - length % spread) / spread * 2 + 1;
        var maxSize = length * maxCols;

        var points = new Coordinate[maxSize];

        var y = 0;
        var x = 0;

        var count = 0;
        for (var i = 0; i < length; i++)
        {
            var row = i + 1;
            int cols;
            if (spread == 1)
            {
                cols = 0;
            }
            else
            {
                // Calculate which band this row belongs to
                // Spread starts expanding when row reaches the spread value
                if (row < spread)
                {
                    cols = 0;
                }
                else
                {
                    cols = (row - spread) / spread + 1;
                }
            }
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