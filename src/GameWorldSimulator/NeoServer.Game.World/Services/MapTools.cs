using System;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.World.Algorithms;

namespace NeoServer.Game.World.Services;

public class MapTool : IMapTool
{
    public MapTool(IMap map, IPathFinder pathFinder)
    {
        PathFinder = pathFinder;
        SightClearChecker = (from, to, checkFloor) =>
            SightClear.IsSightClear(map, from, to, checkFloor);
    }

    public IPathFinder PathFinder { get; }
    public Func<Location, Location, bool, bool> SightClearChecker { get; }
    
    public bool CanThrowObjectTo(Location fromPosition, Location toPosition, SightLine sightLine = SightLine.CheckSightLine, 
        int rangeX = (int)MapViewPort.MaxClientViewPortX , int rangeY = (int) MapViewPort.MaxClientViewPortY)
    {
        if ((fromPosition.IsUnderground && toPosition.IsSurface) || (toPosition.IsUnderground && fromPosition.IsSurface)) {
            return false;
        }
        
        int deltaZ = fromPosition.GetFloorDistanceZ(toPosition);

        if (fromPosition.GetSqmDistanceX(toPosition) - deltaZ > rangeX)
        {
            return false;
        }

        if (fromPosition.GetSqmDistanceY(toPosition) - deltaZ > rangeY)
        {
            return false;
        }

        if ((sightLine & SightLine.CheckSightLine) == 0)
        {
            return true;
        }
        
        return SightClearChecker.Invoke(fromPosition, toPosition, (sightLine & SightLine.FloorCheck) > 0);
    }
}