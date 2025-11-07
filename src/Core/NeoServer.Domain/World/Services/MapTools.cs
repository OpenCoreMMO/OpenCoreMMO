using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.World.Algorithms;

namespace NeoServer.Domain.World.Services;

public class MapTool : IMapTool
{
    private readonly IMap _map;

    public MapTool(IMap map, IPathFinder pathFinder)
    {
        _map = map;
        PathFinder = pathFinder;
        SightClearChecker = (from, to, checkFloor) =>
            SightClear.IsSightClear(map, from, to, checkFloor);
    }

    public IPathFinder PathFinder { get; }
    public Func<Location, Location, bool, bool> SightClearChecker { get; }

    public bool IsClearSight(Location from, Location to, bool checkFloor) => SightClear.IsSightClear(_map, from, to, checkFloor);

    public bool CanThrowObjectTo(Location fromPosition, Location toPosition,
        SightLine sightLine = SightLine.CheckSightLine,
        int rangeX = (int)MapViewPort.MaxClientViewPortX, int rangeY = (int)MapViewPort.MaxClientViewPortY)
    {
        if ((fromPosition.IsUnderground && toPosition.IsSurface) ||
            (toPosition.IsUnderground && fromPosition.IsSurface)) return false;

        var deltaZ = fromPosition.GetFloorDistanceZ(toPosition);

        if (fromPosition.GetSqmDistanceX(toPosition) - deltaZ > rangeX) return false;

        if (fromPosition.GetSqmDistanceY(toPosition) - deltaZ > rangeY) return false;

        if ((sightLine & SightLine.CheckSightLine) == 0) return true;

        return SightClearChecker.Invoke(fromPosition, toPosition, (sightLine & SightLine.FloorCheck) > 0);
    }
}