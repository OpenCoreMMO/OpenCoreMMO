using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.World.Models;

public struct Waypoint : IWaypoint
{
    public string Name { get; set; }
    public Coordinate Coordinate { get; set; }
}