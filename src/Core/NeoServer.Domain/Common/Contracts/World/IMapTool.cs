using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Common.Contracts.World;

public interface IMapTool
{
    IPathFinder PathFinder { get; }
    Func<Location.Structs.Location, Location.Structs.Location, bool, bool> SightClearChecker { get; }

    bool CanThrowObjectTo(Location.Structs.Location fromPosition, Location.Structs.Location toPosition,
        SightLine sightLine = SightLine.CheckSightLine,
        int rangeX = (int)MapViewPort.MaxClientViewPortX, int rangeY = (int)MapViewPort.MaxClientViewPortY);

    bool IsClearSight(Location.Structs.Location from, Location.Structs.Location to, bool checkFloor);
}