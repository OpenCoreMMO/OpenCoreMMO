using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Loaders.OTB.Parsers;
using NeoServer.Loaders.OTB.Structure;

namespace NeoServer.Loaders.OTBM.Structure.Towns;

public class TownNode
{
    public TownNode(OtbNode node)
    {
        var stream = new OtbParsingStream(node.Data);
        Id = stream.ReadUInt32();
        Name = stream.ReadString();

        Coordinate = stream.ReadCoordinate();
    }

    public uint Id { get; set; }
    public string Name { get; set; }
    public Coordinate Coordinate { get; set; }
}