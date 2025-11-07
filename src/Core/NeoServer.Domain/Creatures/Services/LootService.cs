using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Creatures.Monster.Loot;
using NeoServer.Domain.Creatures.Monster.Summon;

namespace NeoServer.Domain.Creatures.Services;

public class LootService(GameConfiguration gameConfiguration, IItemFactory itemFactory) : ILootService
{
    public ILootContainer CreateLootContainer(ICreature deadCreature, IThing killer, decimal lootRate = 0)
    {
        //do not create loot for summons
        if (deadCreature is Summon) return null;
        
        //do not create loot for monsters that are killed by another monster
        if(deadCreature is IMonster && killer is IMonster and not Summon { Master: IPlayer }) return null;

        var loot = GenerateLoot(deadCreature, lootRate);
        var corpse = itemFactory.CreateLootCorpse(deadCreature.CorpseType, deadCreature.Location, loot, killer);
        deadCreature.Corpse = corpse;

        return corpse as ILootContainer;
    }

    public Loot GenerateLoot(ICreature creature, decimal lootRate = 0)
    {
        return creature is IMonster monster ? GenerateLoot(monster, lootRate) : null;
    }

    public Loot GenerateLoot(IMonster monster, decimal lootRate = 0)
    {
        lootRate = lootRate > 0 ? lootRate : gameConfiguration.LootRate;

        var aggressors = GetLootOwners(monster);

        var generateLoot = false;

        //Check stamina of players in the enemies list

        foreach (var aggressor in aggressors)
        {
            var aggressorHasEnoughStamina =
                aggressor is Player.Player { HasLowStamina: false } or Player.Player { IgnoreStamina: true };

            var summonOfAggressorHasEnoughStamina =
                aggressor is Summon { Master: Player.Player { HasLowStamina: false } }
                    or Summon { Master: Player.Player { IgnoreStamina: true } };

            //If the aggressor has enough stamina, generate loot
            if (aggressorHasEnoughStamina || summonOfAggressorHasEnoughStamina || aggressor is IMonster and not Summon)
            {
                generateLoot = true;
            }
        }

        LootItem[] lootItems = null;

        //Only generate loot if there is at least one valid enemy with enough stamina
        if (generateLoot)
        {
            lootItems = GetMonsterLoot(monster.Metadata.Loot.Items, lootRate);
        }

        var loot = new Loot(lootItems ?? [], aggressors);

        monster.RaiseDroppedLootEvent(monster, loot);

        return loot;
    }

    private static HashSet<ICreature> GetLootOwners(IMonster monster)
    {
        var enemies = new HashSet<ICreature>();
        var partyMembers = new List<ICreature>();

        ushort maxDamage = 0;

        foreach (var damageRecord in monster.ReceivedDamages)
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

    private static LootItem[] GetMonsterLoot(LootItem[] items, decimal lootRate)
    {
        var drop = new List<LootItem>();

        foreach (var item in items)
        {
            var random = GameRandom.Random.Next(1, maxValue: 100_000) / lootRate;

            if (item.Chance < random) continue;

            var itemToDrop = item;

            LootItem[] childrenItems = null;
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