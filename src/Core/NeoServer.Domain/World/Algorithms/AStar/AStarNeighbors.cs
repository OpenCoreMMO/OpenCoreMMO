namespace NeoServer.Domain.World.Algorithms.AStar;

internal static class AStarNeighbors
{
    private static readonly sbyte[,] AllNeighbors =
    {
        { -1, 0 }, { 0, 1 }, { 1, 0 }, { 0, -1 }, { -1, -1 }, { 1, -1 }, { 1, 1 }, { -1, 1 }
    };

    public static (int dirCount, sbyte[,] neighbors) GetDirectionsAndNeighbors(Node node)
    {
        if (node.Parent is null) return (8, AllNeighbors);

        var offsetX = node.Parent.X - node.X;
        var offsetY = node.Parent.Y - node.Y;
        return (5, GetNeighbors(offsetY, offsetX));
    }

    private static sbyte[,] GetNeighbors(int offsetY, int offsetX)
    {
        sbyte[,] neighbors;
        if (offsetY == 0)
            neighbors = offsetX == -1 ? NeighborsDirection.West : NeighborsDirection.East;
        else if (offsetX == 0)
            neighbors = offsetY == -1 ? NeighborsDirection.North : NeighborsDirection.South;
        else if (offsetY == -1)
            neighbors = offsetX == -1 ? NeighborsDirection.NorthWest : NeighborsDirection.NorthEast;
        else if (offsetX == -1)
            neighbors = NeighborsDirection.SouthWest;
        else
            neighbors = NeighborsDirection.SouthEast;
        return neighbors;
    }
}