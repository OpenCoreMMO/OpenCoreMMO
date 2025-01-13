using System;
using System.Collections.Generic;
using System.Linq;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Creatures.Monster.Loot;

namespace NeoServer.Game.Creatures.Services;

public class LootService(GameConfiguration gameConfiguration) : ILootService
{
    public ILoot DropLoot(ICreature creature, decimal lootRate = 0) => creature is IMonster monster ? DropLoot(monster, lootRate) : null;

    public ILoot DropLoot(IMonster monster, decimal lootRate = 0)
    {
        lootRate = lootRate > 0 ? lootRate : gameConfiguration.LootRate;

        var lootItems = GetMonsterLoot(monster.Metadata.Loot.Items, lootRate);

        var enemies = GetLootOwners(monster);

        var loot = new Loot(lootItems, Owners: enemies);
        
        monster.RaiseDroppedLootEvent(monster, loot);

        return loot;
    }

    private static HashSet<ICreature> GetLootOwners(IMonster monster)
    {
        var enemies = new HashSet<ICreature>();
        var partyMembers = new List<ICreature>();

        ushort maxDamage = 0;

        foreach (var damageRecord in monster.ReceivedDamages.All)
        {
            if (damageRecord.Aggressor is not ICreature aggressor) continue;
            if (damageRecord.Damage > maxDamage)
            {
                enemies.Clear();
                enemies.Add(aggressor);
                maxDamage = damageRecord.Damage;
                continue;
            }

            if (damageRecord.Damage == maxDamage) enemies.Add(aggressor);
        }

        foreach (var enemy in enemies)
            if (enemy is IPlayer player && player.PlayerParty.Party is not null)
                partyMembers.AddRange(player.PlayerParty.Party.Members);

        return partyMembers.Count == 0 ? enemies.ToHashSet() : enemies.Concat(partyMembers).ToHashSet();
    }

    private ILootItem[] GetMonsterLoot(ILootItem[] items, decimal lootRate)
    {
        var drop = new List<ILootItem>();

        foreach (var item in items)
        {
            var random = GameRandom.Random.Next(1, maxValue: 100_000) / lootRate;

            if (item.Chance < random) continue;

            var itemToDrop = item;

            ILootItem[] childrenItems = null;
            if (item?.Items?.Length > 0) childrenItems = GetMonsterLoot(item.Items, lootRate);

            if (item?.Items?.Length > 0 && childrenItems?.Length == 0) continue;

            var amount = item.Amount;
            if (amount > 1) amount = (byte)(random % item.Amount + 1);

            if (amount == 0) continue;

            itemToDrop = new LootItem(itemToDrop.ItemType, Math.Min(amount, (byte)100), itemToDrop.Chance,
                childrenItems);
            drop.Add(itemToDrop);
        }

        return drop.ToArray();
    }
}