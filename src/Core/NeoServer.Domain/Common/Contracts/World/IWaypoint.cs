using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Common.Contracts.World;

public interface IWaypoint
{
    string Name { get; set; }
    Coordinate Coordinate { get; set; }
}