using System;
using NeoServer.Loaders.OTB.Enums;
using NeoServer.Loaders.OTB.Parsers;
using NeoServer.Loaders.OTB.Structure;

namespace NeoServer.Loaders.OTBM.Structure.TileArea;

public class TileArea
{
    public TileArea(OtbNode node)
    {
        var stream = new OtbParsingStream(node.Data);

        X = stream.ReadUInt16();
        Y = stream.ReadUInt16();
        Z = (sbyte)stream.ReadByte();

        var nodeChildren = node.Children;

        Tiles = new TileNode[nodeChildren.Length];

        var tileArea = this;

        for (var i = 0; i < nodeChildren.Length; i++)
        {
            var child = nodeChildren.Span[i];

            if (child.Type is not NodeType.HouseTile && child.Type is not NodeType.NormalTile)
                throw new Exception("unknown tile nodes found.");

            var tileNode = new TileNode(tileArea, child);

            Tiles[i] = tileNode;
        }
    }

    public ushort X { get; }
    public ushort Y { get; }
    public sbyte Z { get; }

    public TileNode[] Tiles { get; }
}