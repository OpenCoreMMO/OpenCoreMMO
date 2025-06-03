using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.Common.Contracts.World;

public interface ITown
{
    uint Id { get; set; }
    string Name { get; set; }
    Coordinate Coordinate { get; set; }
}