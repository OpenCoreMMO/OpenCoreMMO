using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Loaders.OTB.Parsers;
using NeoServer.Loaders.OTB.Structure;

namespace NeoServer.Loaders.OTBM.Structure;

public class WaypointNode
{
    public string Name { get; set; }
    public Coordinate Coordinate { get; set; }

    public WaypointNode(OtbNode node)
    {
        var stream = new OtbParsingStream(node.Data);

        Name = stream.ReadString();

        Coordinate = stream.ReadCoordinate();
    }
}