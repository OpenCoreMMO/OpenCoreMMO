using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Item.Structs;
using NeoServer.Domain.Items;
using NeoServer.Loaders.OTB.Structure;

namespace NeoServer.Loaders.Items.Parsers;

public static class ItemNodeParser
{
    /// <summary>
    ///     Parses ItemNode object to IItemType
    /// </summary>
    /// <param name="itemNode"></param>
    /// <returns></returns>
    public static IItemType Parse(ItemNode itemNode)
    {
        var itemType = new ItemType();

        itemType.SetId(itemNode.ServerId);
        itemType.SetClientId(itemNode.ClientId);
        itemType.SetSpeed(itemNode.Speed);
        itemType.SetLight(new LightBlock(itemNode.LightLevel, itemNode.LightColor));
        itemType.SetGroup((byte)itemNode.Type);

        itemType.ParseOTFlags(itemNode.Flags);

        return itemType;
    }
}