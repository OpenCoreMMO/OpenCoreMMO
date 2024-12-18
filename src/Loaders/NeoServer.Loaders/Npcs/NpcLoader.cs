using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Creatures.Npcs;
using NeoServer.Game.Creatures.Npcs.Shop;
using NeoServer.Loaders.Interfaces;
using NeoServer.Server.Configurations;
using NeoServer.Server.Helpers.Extensions;
using System.Text.Json;
using Serilog;

namespace NeoServer.Loaders.Npcs;

public class NpcLoader : IStartupLoader
{
    private readonly IItemTypeStore _itemTypeStore;
    private readonly INpcStore _npcStore;
    private readonly ILogger logger;
    private readonly ServerConfiguration serverConfiguration;

    public NpcLoader(ServerConfiguration serverConfiguration, ILogger logger, INpcStore npcStore,
        IItemTypeStore itemTypeStore)
    {
        this.serverConfiguration = serverConfiguration;
        this.logger = logger;
        _npcStore = npcStore;
        _itemTypeStore = itemTypeStore;
    }

    public void Load()
    {
        logger.Step("Loading npcs...", "{n} npcs loaded", () =>
        {
            var npcs = ConvertNpcs().ToArray();

            foreach (var npcLoaded in npcs)
            {
                var (jsonContent, npc) = npcLoaded;
                _npcStore.Add(npc.Name, npc);
                OnLoad?.Invoke(npc, jsonContent);
            }

            return new object[] { npcs.Length };
        });
    }

    public event Action<INpcType, string> OnLoad;

    private IEnumerable<(string, INpcType)> ConvertNpcs()
    {
        var basePath = $"{serverConfiguration.Data}/npcs";
        foreach (var file in Directory.GetFiles(basePath, "*.json"))
        {
            var jsonContent = File.ReadAllText(file);
            NpcJsonData npcData = null;
            try
            {
                npcData = JsonSerializer.Deserialize<NpcJsonData>(jsonContent);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                continue;
            }

            if (npcData is null) continue;
            if (string.IsNullOrWhiteSpace(npcData.Name)) continue;

            var dialogs = new List<IDialog>();

            foreach (var dialog in npcData.Dialog) dialogs.Add(ConvertDialog(dialog));

            var npcType = (jsonContent, new NpcType
            {
                Script = npcData.Script,
                MaxHealth = npcData.Health?.Max ?? 100,
                Name = npcData.Name,
                Marketings = npcData.Marketings,
                WalkInterval = (uint)Math.Max(2_000, npcData.WalkInterval),
                Speed = 120,
                Look = new Dictionary<LookType, ushort>
                {
                    { LookType.Type, npcData.Look.Type }, { LookType.Corpse, npcData.Look.Corpse },
                    { LookType.Body, npcData.Look.Body }, { LookType.Legs, npcData.Look.Legs },
                    { LookType.Head, npcData.Look.Head },
                    { LookType.Feet, npcData.Look.Feet }, { LookType.Addon, npcData.Look.Addons }
                },
                Dialogs = dialogs.ToArray()
            });

            LoadShopData(npcType.Item2, npcData);

            NpcCustomAttributeLoader.LoadCustomData(npcType.Item2, npcData);

            yield return npcType;
        }
    }

    private void LoadShopData(INpcType type, NpcJsonData npcData)
    {
        if (type is null || npcData?.Shop is null) return;

        var items = new Dictionary<ushort, IShopItem>(npcData.Shop.Length);
        foreach (var item in npcData.Shop)
        {
            if (!_itemTypeStore.TryGetValue(item.Item, out var itemType)) continue;
            items.Add(itemType.TypeId, new ShopItem(itemType, item.Buy, item.Sell));
        }

        type.CustomAttributes.Add("shop", items);
    }

    private static IDialog ConvertDialog(NpcJsonData.DialogData dialog)
    {
        if (dialog is null) return null;
        var d = new Dialog
        {
            Back = dialog.Back,
            Answers = dialog.Answers,
            Action = dialog.Action,
            OnWords = dialog.Words,
            End = dialog.End,
            StoreAt = dialog.StoreAt,
            Then = dialog.Then?.Select(ConvertDialog).ToArray()
        };
        return d;
    }
}