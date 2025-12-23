using System;
using System.Collections.Generic;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Loaders.OTB.Enums;
using NeoServer.Loaders.OTB.Parsers;
using NeoServer.Loaders.OTB.Structure;
using NeoServer.Loaders.OTBM.Enums;

namespace NeoServer.Loaders.OTBM.Structure.TileArea;

public class TileNode : ITileNode
{
    public TileNode(TileArea tileArea, OtbNode node)
    {
        var children = node.Children;
        Items = new List<ItemNode>(children.Length);
        NodeAttribute = NodeAttribute.None;
        Flag = TileFlags.None;
        HouseId = 0;

        var stream = new OtbParsingStream(node.Data);

        var x = (ushort)(tileArea.X + stream.ReadByte());
        var y = (ushort)(tileArea.Y + stream.ReadByte());

        Coordinate = new Coordinate(x, y, tileArea.Z);

        if (node.Type == NodeType.HouseTile) HouseId = stream.ReadUInt32();

        NodeType = node.Type;

        ParseAttributes(stream);

        foreach (var child in children.Span) Items.Add(new ItemNode(this, child));
    }

    public Coordinate Coordinate { get; }
    public NodeType NodeType { get; }
    public TileFlags Flag { get; private set; }
    public List<ItemNode> Items { get; }
    public uint HouseId { get; }

    private NodeAttribute NodeAttribute { get; set; }
    private bool IsFlag => NodeAttribute == NodeAttribute.TileFlags;
    private bool IsItem => NodeAttribute == NodeAttribute.Item;

    private void ParseAttributes(OtbParsingStream stream)
    {
        while (!stream.IsOver)
        {
            NodeAttribute = (NodeAttribute)stream.ReadByte();

            if (IsFlag)
                Flag = ParseTileFlags((OTBMTileFlags)stream.ReadUInt32());
            else if (IsItem)
                Items.Add(new ItemNode(stream));
            else
                throw new Exception($"{Coordinate}: Unknown tile attribute");
        }
    }

    private static TileFlags ParseTileFlags(OTBMTileFlags newFlags)
    {
        var oldFlags = TileFlags.None;

        if ((newFlags & OTBMTileFlags.ProtectionZone) != 0)
            oldFlags |= TileFlags.ProtectionZone;
        else if ((newFlags & OTBMTileFlags.NoPvpZone) != 0)
            oldFlags |= TileFlags.NoPvpZone;
        else if ((newFlags & OTBMTileFlags.PvpZone) != 0)
            oldFlags |= TileFlags.PvpZone;

        if ((newFlags & OTBMTileFlags.NoLogout) != 0)
            oldFlags |= TileFlags.NoLogout;

        return oldFlags;
    }
}