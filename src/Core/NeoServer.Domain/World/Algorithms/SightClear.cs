using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.World.Algorithms;

public static class SightClear
{
    /// <summary>
    ///     Checks if the sight is clear from location to target
    /// </summary>
    public static bool IsSightClear(IMap map, Location fromPosition, Location toPosition, bool checkFloor)
    {
        if (checkFloor && fromPosition.Z != toPosition.Z) return false;

        // Cast two converging rays and see if either yields a result.
        return CheckSightLine(map, fromPosition, toPosition) || CheckSightLine(map, toPosition, fromPosition);
    }

    private static bool CheckSightLine(IMap map, Location fromPosition, Location toPosition)
    {
        if (toPosition.Type == LocationType.Ground)
        {
            if (fromPosition == toPosition)
                return true;
        }

        if (fromPosition.IsNextTo(toPosition)) return true;

        var start = fromPosition.Z > toPosition.Z ? toPosition : fromPosition;
        var destination = fromPosition.Z > toPosition.Z ? fromPosition : toPosition;

        var mx = start.X < destination.X ? 1 : start.X == destination.X ? 0 : -1;
        var my = start.Y < destination.Y ? 1 : start.Y == destination.Y ? 0 : -1;

        var offset = Location.GetOffsetBetween(destination, start);

        var a = offset[1];
        var b = offset[0];
        var c = -(a * destination.X + b * destination.Y);

        while (start.X != destination.X || start.Y != destination.Y)
        {
            var moveHorizontal = Math.Abs(a * (start.X + mx) + b * start.Y + c);
            var moveVertical = Math.Abs(a * start.X + b * (start.Y + my) + c);
            var moveCross = Math.Abs(a * (start.X + mx) + b * (start.Y + my) + c);

            if (start.Y != destination.Y &&
                (start.X == destination.X || moveHorizontal > moveVertical || moveHorizontal > moveCross))
                start.Y += (ushort)my;

            if (start.X != destination.X &&
                (start.Y == destination.Y || moveVertical > moveHorizontal || moveVertical > moveCross))
                start.X += (ushort)mx;

            var tile = map[start.X, start.Y, start.Z];
            if (tile is { BlockMissile: true }) return false;
        }

        // now we need to perform a jump between floors to see if everything is clear (literally)
        while (start.Z != destination.Z)
        {
            var tile = map[start.X, start.Y, start.Z];

            if (tile is { HasThings: true }) return false;

            start.Z++;
        }

        return true;
    }
}