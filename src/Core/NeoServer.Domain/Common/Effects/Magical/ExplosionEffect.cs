using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Common.Effects.Magical;

public static class ExplosionEffect
{
    private static readonly byte[,] Area =
    {
        { 0, 0, 0, 0, 0, 0, 8, 0, 0, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 8, 8, 7, 8, 8, 0, 0, 0, 0 },
        { 0, 0, 0, 8, 7, 6, 6, 6, 7, 8, 0, 0, 0 },
        { 0, 0, 8, 7, 6, 5, 5, 5, 6, 7, 8, 0, 0 },
        { 0, 8, 7, 6, 5, 4, 4, 4, 5, 6, 7, 8, 0 },
        { 0, 8, 6, 5, 4, 3, 2, 3, 4, 5, 6, 8, 0 },
        { 8, 7, 6, 5, 4, 2, 1, 2, 4, 5, 6, 7, 8 },
        { 0, 8, 6, 5, 4, 3, 2, 3, 4, 5, 6, 8, 0 },
        { 0, 8, 7, 6, 5, 4, 4, 4, 5, 6, 7, 8, 0 },
        { 0, 0, 8, 7, 6, 5, 5, 5, 6, 7, 8, 0, 0 },
        { 0, 0, 0, 8, 7, 6, 6, 6, 7, 8, 0, 0, 0 },
        { 0, 0, 0, 0, 8, 8, 7, 8, 8, 0, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 0, 8, 0, 0, 0, 0, 0, 0 }
    };

    public static IEnumerable<Coordinate> Create(int radius)
    {
        var points = new List<Coordinate>();

        for (var row = 0; row < 13; row++)
        for (var col = 0; col < 13; col++)
            if (Area[row, col] > 0 && Area[row, col] <= radius)
            {
                var y = row - 6;
                var x = col - 6;
                points.Add(new Coordinate(x, y, 0));
            }

        return points;
    }

    public static IEnumerable<Coordinate> Create(Location.Structs.Location location, int radius)
    {
        var i = 0;

        var affectedLocations = Create(radius);
        var coordinates = affectedLocations.ToList();
        var affectedArea = new Coordinate[coordinates.Count];

        foreach (var affectedLocation in coordinates)
            affectedArea[i++] = location.Translate() + affectedLocation;
        return affectedArea;
    }
}