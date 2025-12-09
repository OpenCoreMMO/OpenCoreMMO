using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Location.Structs;

namespace NeoServer.Domain.World.Models;

public class Town : ITown
{
    public uint Id { get; set; }
    public string Name { get; set; }
    public Coordinate Coordinate { get; set; }
}