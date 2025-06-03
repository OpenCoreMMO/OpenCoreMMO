using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.World.Algorithms.AStar;

internal static class AStarDirections
{
    private static readonly Stack<Direction> DirList = new();

    public static Direction[] GetAll(Node node, Location startPos, Location endPos)
    {
        DirList.Clear();
        var prevPos = new Location(endPos.X, endPos.Y, endPos.Z);
        node = node.Parent;

        while (node is not null)
        {
            var pos = new Location(node.X, node.Y, startPos.Z);

            var direction = Next(prevPos, pos);
            DirList.Push(direction);

            prevPos = pos;
            node = node.Parent;
        }

        return DirList.ToArray();
    }

    private static Direction Next(Location prevPos, Location pos)
    {
        var dx = pos.X - prevPos.X;
        var dy = pos.Y - prevPos.Y;

        switch (dx)
        {
            case 1 when dy == 1:
                return Direction.NorthWest;
            case 1 when dy == -1:
                return Direction.SouthWest;
            case 1:
                return Direction.West;
            case -1 when dy == 1:
                return Direction.NorthEast;
            case -1 when dy == -1:
                return Direction.SouthEast;
            case -1:
                return Direction.East;
            default:
            {
                switch (dy)
                {
                    case 1:
                        return Direction.North;
                    case -1:
                        return Direction.South;
                }

                break;
            }
        }

        return Direction.None;
    }
}