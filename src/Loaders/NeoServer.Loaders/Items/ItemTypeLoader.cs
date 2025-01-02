using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text.Json;
using NeoServer.Data.InMemory.DataStores;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Item;
using NeoServer.Loaders.Items.Parsers;
using NeoServer.Loaders.OTB.Parsers;
using NeoServer.Loaders.OTB.Structure;
using NeoServer.Server.Configurations;
using NeoServer.Server.Helpers.Extensions;
using Serilog;

namespace NeoServer.Loaders.Items
{
    public class ItemTypeLoader
    {
        private readonly IItemClientServerIdMapStore _itemClientServerIdMapStore;

        private readonly IItemTypeStore _itemTypeStore;
        private readonly ILogger _logger;
        private readonly ServerConfiguration _serverConfiguration;
        
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            DefaultBufferSize = 4096,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
        };

        public ItemTypeLoader(ILogger logger, ServerConfiguration serverConfiguration, IItemTypeStore itemTypeStore,
            IItemClientServerIdMapStore itemClientServerIdMapStore)
        {
            _logger = logger;
            _serverConfiguration = serverConfiguration;
            _itemTypeStore = itemTypeStore;
            _itemClientServerIdMapStore = itemClientServerIdMapStore;
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

                LoadItemsJson(basePath, itemTypes);

                foreach (var item in itemTypes.OrderBy(x => x.Key))
                {
                    _itemTypeStore.Add(item.Key, item.Value);
                    _itemClientServerIdMapStore.Add(item.Value.ClientId, item.Key);

                    if (item.Value.Attributes.GetAttribute(ItemAttribute.Type)
                            ?.Equals("coin", StringComparison.InvariantCultureIgnoreCase) ?? false)
                    {
                        CoinTypeStore.Data.Add(item.Key, item.Value);
                    }
                }

                return new object[] { itemTypes.Count };
            });
        }

        private Dictionary<ushort, IItemType> LoadOtb(string basePath)
        {
            var fileStream = File.ReadAllBytes(Path.Combine(basePath, _serverConfiguration.OTB));

            var otbNode = OtbBinaryTreeBuilder.Deserialize(fileStream);
            var otb = new Otb(otbNode);

        var itemTypes = otb.ItemNodes.AsParallel().Select(ItemNodeParser.Parse).ToDictionary(x => x.ServerId);
        return itemTypes;
    }

        private static void LoadItemsJson(string basePath, IDictionary<ushort, IItemType> itemTypes)
        {
            var itemTypeMetadata = GetItemTypeMetadataList(basePath);

            var itemTypeMetadataParser = new ItemTypeMetadataParser(itemTypes);

            (itemTypeMetadata ?? Array.Empty<ItemTypeMetadata>()).AsParallel().ForAll(metadata =>
            {
                if (metadata.Id.HasValue)
                {
                    itemTypeMetadataParser.AddMetadata(metadata, metadata.Id.Value);
                    return;
                }

                if (metadata.Fromid == null)
                {
                    Console.WriteLine("No item id found");
                    return;
                }

                if (metadata.Toid == null)
                {
                    Console.WriteLine($"fromid ({metadata.Fromid}) without toid");
                    return;
                }

                var id = metadata.Fromid.Value;
                while (id <= metadata.Toid) itemTypeMetadataParser.AddMetadata(metadata, id++);
            });
        }

        private static IEnumerable<ItemTypeMetadata> GetItemTypeMetadataList(string basePath)
        {
            using var memoryMappedFile = MemoryMappedFile.CreateFromFile(Path.Combine(basePath, "items.json"));
            using var stream = memoryMappedFile.CreateViewStream();
            using var reader = new StreamReader(stream);

            return JsonSerializer.Deserialize<IEnumerable<ItemTypeMetadata>>(reader.ReadToEnd().Trim('\0'), _jsonOptions);
        }
    }
}
