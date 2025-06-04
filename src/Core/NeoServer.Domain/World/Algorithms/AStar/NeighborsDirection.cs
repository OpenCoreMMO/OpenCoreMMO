namespace NeoServer.Domain.World.Algorithms.AStar;

internal readonly record struct NeighborsDirection
{
    public static sbyte[,] West => new sbyte[,]
    {
        { 0, 1 },
        { 1, 0 },
        { 0, -1 },
        { 1, -1 },
        { 1, 1 }
    };

    public static sbyte[,] East => new sbyte[,]
    {
        { -1, 0 },
        { 0, 1 },
        { 0, -1 },
        { -1, -1 },
        { -1, 1 }
    };

    public static sbyte[,] North => new sbyte[,]
    {
        { -1, 0 },
        { 0, 1 },
        { 1, 0 },
        { 1, 1 },
        { -1, 1 }
    };

    public static sbyte[,] South => new sbyte[,]
    {
        { -1, 0 },
        { 1, 0 },
        { 0, -1 },
        { -1, -1 },
        { 1, -1 }
    };

    public static sbyte[,] NorthWest => new sbyte[,]
    {
        { 0, 1 },
        { 1, 0 },
        { 1, -1 },
        { 1, 1 },
        { -1, 1 }
    };

    public static sbyte[,] NorthEast => new sbyte[,]
    {
        { -1, 0 },
        { 0, 1 },
        { -1, -1 },
        { 1, 1 },
        { -1, 1 }
    };

    public static sbyte[,] SouthWest => new sbyte[,]
    {
        { 1, 0 },
        { 0, -1 },
        { -1, -1 },
        { 1, -1 },
        { 1, 1 }
    };

    public static sbyte[,] SouthEast => new sbyte[,]
    {
        { -1, 0 },
        { 0, -1 },
        { -1, -1 },
        { 1, -1 },
        { -1, 1 }
    };
}