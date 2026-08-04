using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NeoServer.Loaders.OTB.Enums;
using NeoServer.Loaders.OTB.Parsers;
using NeoServer.Loaders.OTB.Structure;
using NeoServer.Loaders.OTBM.Enums;

namespace NeoServer.Loaders.OTBM.Structure.TileArea;

public class ItemNode
{
    public ItemNode(OtbParsingStream stream)
    {
        ItemNodeAttributes = null;
        ItemId = 0;
        Children = [];
        ItemId = ParseItemId(stream);
    }

    public ItemNode(TileNode tile, OtbNode node)
    {
        ItemNodeAttributes = [];
        ItemId = 0;

        var nodeChildren = node.Children;
        Children = new List<ItemNode>(nodeChildren.Length);

        if (node.Type != NodeType.Item) throw new Exception($"{tile.Coordinate}: Unknown node type");

        var stream = new OtbParsingStream(node.Data);

        ItemId = ParseItemId(stream);

        ParseAttributes(stream);

        AddChildren(tile, nodeChildren.Span);
    }

    public ushort ItemId { get; }
    public List<ItemNodeAttributeValue> ItemNodeAttributes { get; }
    public List<ItemNode> Children { get; }

    private void AddChildren(TileNode tileNode, ReadOnlySpan<OtbNode> nodeChildren)
    {
        foreach (var nodeChild in nodeChildren)
        {
            if (nodeChild.Type is not NodeType.Item) continue;

            Children.Add(new ItemNode(tileNode, nodeChild));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ushort ParseItemId(OtbParsingStream stream)
    {
        var originalItemId = stream.ReadUInt16();

        var parsedItemId = originalItemId switch
        {
            (ushort)OTBMWorldItemId.FireFieldPvpLarge => (ushort)OTBMWorldItemId.FireFieldPersistentLarge,
            (ushort)OTBMWorldItemId.FireFieldPvpMedium => (ushort)OTBMWorldItemId.FireFieldPersistentMedium,
            (ushort)OTBMWorldItemId.FireFieldPvpSmall => (ushort)OTBMWorldItemId.FireFieldPersistentSmall,
            (ushort)OTBMWorldItemId.EnergyFieldPvp => (ushort)OTBMWorldItemId.EnergyFieldPersistent,
            (ushort)OTBMWorldItemId.PoisonFieldPvp => (ushort)OTBMWorldItemId.PoisonFieldPersistent,
            (ushort)OTBMWorldItemId.MagicWall => (ushort)OTBMWorldItemId.MagicWallPersistent,
            (ushort)OTBMWorldItemId.WildGrowth => (ushort)OTBMWorldItemId.WildGrowthPersistent,
            _ => originalItemId
        };

        return parsedItemId;
    }

    private void ParseAttributes(OtbParsingStream stream)
    {
        while (!stream.IsOver)
        {
            var attribute = stream.ReadByte();

            if (attribute == 0) break;

            ItemNodeAttributes.Add(new ItemNodeAttributeValue((ItemNodeAttribute)attribute, stream));
        }
    }
}