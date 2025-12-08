using System;
using System.Collections.Generic;
using NeoServer.Loaders.OTB.Enums;
using NeoServer.Loaders.OTB.Parsers;
using NeoServer.Loaders.OTB.Structure;
using NeoServer.Loaders.OTBM.Enums;

namespace NeoServer.Loaders.OTBM.Structure.TileArea;

public class ItemNode
{
    public ushort ItemId { get; set; }
    public List<ItemNodeAttributeValue> ItemNodeAttributes { get; }
    public List<ItemNode> Children { get; }

    public ItemNode(OtbParsingStream stream)
    {
        ItemNodeAttributes = default;
        ItemId = default;
        Children = new List<ItemNode>(0);
        ItemId = ParseItemId(stream);
    }

    public ItemNode(TileNode tile, OtbNode node)
    {
        ItemNodeAttributes = new List<ItemNodeAttributeValue>();
        ItemId = default;
        Children = new List<ItemNode>(0);

        if (node.Type != NodeType.Item) throw new Exception($"{tile.Coordinate}: Unknown node type");

        var stream = new OtbParsingStream(node.Data);

        ItemId = ParseItemId(stream);

        ParseAttributes(stream);

        AddChildren(tile, node);
    }

    private void AddChildren(TileNode tileNode, OtbNode node)
    {
        foreach (var nodeChild in node.Children)
        {
            if (nodeChild.Type is not NodeType.Item) continue;

            Children.Add(new ItemNode(tileNode, nodeChild));
        }
    }

    private ushort ParseItemId(OtbParsingStream stream)
    {
        var originalItemId = stream.ReadUInt16();
        var parsedItemId = originalItemId;

        switch (originalItemId)
        {
            case (ushort)OTBMWorldItemId.FireFieldPvpLarge:
                parsedItemId = (ushort)OTBMWorldItemId.FireFieldPersistentLarge;
                break;

            case (ushort)OTBMWorldItemId.FireFieldPvpMedium:
                parsedItemId = (ushort)OTBMWorldItemId.FireFieldPersistentMedium;
                break;

            case (ushort)OTBMWorldItemId.FireFieldPvpSmall:
                parsedItemId = (ushort)OTBMWorldItemId.FireFieldPersistentSmall;
                break;

            case (ushort)OTBMWorldItemId.EnergyFieldPvp:
                parsedItemId = (ushort)OTBMWorldItemId.EnergyFieldPersistent;
                break;

            case (ushort)OTBMWorldItemId.PoisonFieldPvp:
                parsedItemId = (ushort)OTBMWorldItemId.PoisonFieldPersistent;
                break;

            case (ushort)OTBMWorldItemId.MagicWall:
                parsedItemId = (ushort)OTBMWorldItemId.MagicWallPersistent;
                break;

            case (ushort)OTBMWorldItemId.WildGrowth:
                parsedItemId = (ushort)OTBMWorldItemId.WildGrowthPersistent;
                break;
        }

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