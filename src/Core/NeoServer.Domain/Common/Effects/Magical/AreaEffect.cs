using System.Buffers;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Common.Effects.Magical;

public static partial class AreaEffect
{
    public static Coordinate[] Create(Location.Structs.Location location, byte[,] areaTemplate)
    {
        if (areaTemplate is null) return Array.Empty<Coordinate>();

        var rows = areaTemplate.GetLength(0);
        var columns = areaTemplate.GetLength(1);

        var pool = ArrayPool<Coordinate>.Shared;
        var coordinates = pool.Rent(rows * columns);

        var pointList = Scan(areaTemplate);

        var origin = pointList.Origin;

        var count = 1;

        coordinates[0] = new Coordinate(location);
        Parallel.ForEach(pointList.Points, affectedLocation =>
        {
            var x = location.X + (affectedLocation.Item1 - origin.Item1);
            var y = location.Y + (affectedLocation.Item2 - origin.Item2);

            coordinates[count++] = new Coordinate(x, y, (sbyte)location.Z);
        });

        pool.Return(coordinates);

        return coordinates[..count];
    }

    private static PointList Scan(byte[,] array)
    {
        var rows = array.GetLength(0);
        var columns = array.GetLength(1);

        var points = new PointList(rows * columns);

        Parallel.For(0, rows, i =>
        {
            for (byte j = 0; j < columns; j++)
                switch (array[i, j])
                {
                    case 1:
                        points.AddPoint((byte)i, j);
                        break;
                    case 3:
                        points.AddOrigin((byte)i, j);
                        break;
                    default: continue;
                }
        });

        return points;
    }

    private class PointList
    {
        public PointList(int maxSize)
        {
            Points = new List<(byte, byte)>(maxSize);
            Origin = (0, 0);
        }

        public (byte, byte) Origin { get; private set; }
        public List<(byte, byte)> Points { get; }

        public void AddOrigin(byte rowIndex, byte columnIndex)
        {
            Origin = (rowIndex, columnIndex);
        }

        public void AddPoint(byte rowIndex, byte columnIndex)
        {
            Points.Add((rowIndex, columnIndex));
        }
    }
}