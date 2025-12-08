using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Items;
using NeoServer.Loaders.Items.Parsers;
using NeoServer.Loaders.OTB.Parsers;
using NeoServer.Loaders.OTB.Structure;
using NeoServer.Server.Configurations;
using NeoServer.Server.Helpers.Extensions;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text.Json;

namespace NeoServer.Loaders.Items;

public class ItemTypeLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultBufferSize = 4096,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private readonly ICoinTypeStore _coinTypeStore;

    private readonly IItemClientServerIdMapStore _itemClientServerIdMapStore;

    private readonly IItemTypeStore _itemTypeStore;
    private readonly ILogger _logger;
    private readonly ServerConfiguration _serverConfiguration;


    public ItemTypeLoader(
        ILogger logger,
        ServerConfiguration serverConfiguration,
        IItemTypeStore itemTypeStore,
        IItemClientServerIdMapStore itemClientServerIdMapStore,
        ICoinTypeStore coinTypeStore)
    {
        _logger = logger;
        _serverConfiguration = serverConfiguration;
        _itemTypeStore = itemTypeStore;
        _itemClientServerIdMapStore = itemClientServerIdMapStore;
        _coinTypeStore = coinTypeStore;
    }

    /// <summary>
    ///     Loads the OTB and XML files into a collection of ItemType objects
    /// </summary>
    public void Load()
    {
        _logger.Step("Loading items", "{n} items loaded", () =>
        {
            var basePath = $"{_serverConfiguration.Data}/items/";
            var itemTypes = LoadOtb(basePath);

            LoadItemsJson(basePath, itemTypes, _logger);

            foreach (var item in itemTypes)
            {
                _itemTypeStore.AddOrUpdate(item.Key, item.Value);
                _itemClientServerIdMapStore.AddOrUpdate(item.Value.ClientId, item.Key);

                if (item.Value.Attributes.GetAttribute(ItemTypeAttribute.Type)
                        ?.Equals("coin", StringComparison.InvariantCultureIgnoreCase) ?? false)
                {
                    _coinTypeStore.AddOrUpdate(item.Key, item.Value);
                }
            }

            return [itemTypes.Count];
        });
    }

    private Dictionary<ushort, IItemType> LoadOtb(string basePath)
    {
        var fileStream = File.ReadAllBytes(Path.Combine(basePath, _serverConfiguration.OTB));

        var otbNode = OtbBinaryTreeBuilder.Deserialize(fileStream);
        var otb = new Otb(otbNode);

        var items = otb.ItemNodes;
        var itemTypes = new Dictionary<ushort, IItemType>(items.Count);

        foreach (var node in items)
        {
            var parsed = ItemNodeParser.Parse(node);
            itemTypes[parsed.ServerId] = parsed;
        }

        return itemTypes;
    }

    private static void LoadItemsJson(string basePath, IDictionary<ushort, IItemType> itemTypes, ILogger logger)
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

    private static List<ItemTypeMetadata> GetItemTypeMetadataList(string basePath)
    {
        using var stream = File.OpenRead(Path.Combine(basePath, "items.json"));
        return JsonSerializer.Deserialize<List<ItemTypeMetadata>>(stream, JsonOptions) ?? [];
    }
}