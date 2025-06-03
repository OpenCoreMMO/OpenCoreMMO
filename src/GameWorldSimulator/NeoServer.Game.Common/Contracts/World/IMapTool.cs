using System;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;

namespace NeoServer.Game.Common.Contracts.World;

public interface IMapTool
{
    IPathFinder PathFinder { get; }
    Func<Location.Structs.Location, Location.Structs.Location, bool, bool> SightClearChecker { get; }

    bool CanThrowObjectTo(Location.Structs.Location fromPosition, Location.Structs.Location toPosition, SightLine sightLine = SightLine.CheckSightLine,
        int rangeX = (int)MapViewPort.MaxClientViewPortX, int rangeY = (int)MapViewPort.MaxClientViewPortY);
}