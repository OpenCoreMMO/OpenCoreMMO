using System.Collections.Generic;
using System.Linq;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Game.Creatures.Monster.Loot;
using static NeoServer.Loaders.Monsters.MonsterData;

namespace NeoServer.Loaders.Monsters.Converters;

public static class MonsterLootConverter
{
    public static ILoot Convert(MonsterData data, IItemTypeStore itemTypeStore)
    {
        if (data.Loot is null) return null;

        var items = new List<ILootItem>();

        foreach (var item in Normalize(data.Loot)) items.Add(ConvertToLootItem(item, itemTypeStore));

        return new Loot(items.ToArray());
    }

    private static List<LootData> Normalize(List<LootData> lootData)
    {
        return lootData?.GroupBy(x => x.Id)?.Select(gp => new LootData
        {
            Chance = gp.FirstOrDefault().Chance,
            Id = gp.Key,
            Countmax = gp.Sum(s => byte.TryParse(s.Countmax, out var amount) ? amount : 0).ToString(),
            Items = Normalize(gp.FirstOrDefault().Items)
        })?.ToList();
    }

    private static ILootItem ConvertToLootItem(LootData item, IItemTypeStore itemTypeStore)
    {
        byte.TryParse(item.Countmax, out var amount);
        ushort.TryParse(item.Id, out var id);
        uint.TryParse(item.Chance, out var chance);

        var items = new List<ILootItem>();

        if (item?.Items?.Count > 0)
            foreach (var child in item?.Items)
                items.Add(ConvertToLootItem(child, itemTypeStore));

        return new LootItem(itemTypeStore.Get(id), amount == 0 ? (byte)1 : amount, chance, items.ToArray());
    }
}