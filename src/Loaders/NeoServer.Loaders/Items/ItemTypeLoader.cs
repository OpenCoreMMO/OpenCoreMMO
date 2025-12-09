using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Item;
using NeoServer.Loaders.Items.Parsers;
using NeoServer.Loaders.OTB.Parsers;
using NeoServer.Loaders.OTB.Structure;
using NeoServer.Server.Configurations;
using NeoServer.Server.Helpers.Extensions;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using NeoServer.Loaders.Helpers;

namespace NeoServer.Loaders.Items;

public class ItemTypeLoader(
    ILogger logger,
    ServerConfiguration serverConfiguration,
    IItemTypeStore itemTypeStore,
    IItemClientServerIdMapStore itemClientServerIdMapStore,
    ICoinTypeStore coinTypeStore)
{
    /// <summary>
    /// Loads the OTB and XML files into a collection of ItemType objects
    /// </summary>
    public void Load()
    {
        logger.Step("Loading items", "{n} item types loaded", () =>
        {
            var basePath = $"{serverConfiguration.Data}/items/";
            var itemTypes = LoadOtb(basePath);

            LoadItemsJson(basePath, itemTypes, logger);

            foreach (var item in itemTypes)
            {
                itemTypeStore.AddOrUpdate(item.Key, item.Value);
                itemClientServerIdMapStore.AddOrUpdate(item.Value.ClientId, item.Key);

                if (item.Value.Attributes.GetAttribute(ItemTypeAttribute.Type)
                        ?.Equals("coin", StringComparison.InvariantCultureIgnoreCase) ?? false)
                {
                    coinTypeStore.AddOrUpdate(item.Key, item.Value);
                }
            }

            return [itemTypes.Count];
        });
    }

    private Dictionary<ushort, IItemType> LoadOtb(string basePath)
    {
        var fileStream = File.ReadAllBytes(Path.Combine(basePath, serverConfiguration.OTB));

        var otbNode = OtbBinaryTreeBuilder.Deserialize(fileStream);
        var otb = new Otb(otbNode);

        var items = otb.ItemNodes;
        var itemTypes = new Dictionary<ushort, IItemType>(items.Length);

        foreach (var node in items)
        {
            var parsed = ItemNodeParser.Parse(node);
            itemTypes[parsed.ServerId] = parsed;
        }

        return itemTypes;
    }

    private static void LoadItemsJson(string basePath, Dictionary<ushort, IItemType> itemTypes, ILogger logger)
    {
        var itemTypeMetadata = GetItemTypeMetadataList(basePath);

        var itemTypeMetadataParser = new ItemTypeMetadataParser(itemTypes);

        foreach (var metadata in itemTypeMetadata)
        {
            if (metadata.Id.HasValue)
            {
                itemTypeMetadataParser.AddMetadata(metadata, metadata.Id.Value);
                continue;
            }
            if (metadata.Fromid == null)
            {
                logger.Warning("No item found");
                continue;
            }
            if (metadata.Toid == null)
            {
                logger.Warning("fromId ({MetadataFromId}) without toId", metadata.Fromid);
                continue;
            }
            var id = metadata.Fromid.Value;
            while (id <= metadata.Toid) itemTypeMetadataParser.AddMetadata(metadata, id++);
        }
    }

    private static ItemTypeMetadata[] GetItemTypeMetadataList(string basePath)
    {
        using var stream = File.OpenRead(Path.Combine(basePath, "items.json"));
        return JsonSerializer.Deserialize<ItemTypeMetadata[]>(stream, JsonSettings.Options) ?? [];
    }
}