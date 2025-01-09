using System.Collections.Generic;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Item;

namespace NeoServer.Game.Creatures.Services;

public class CreatureDeathService(
    IItemFactory itemFactory,
    IMap map,
    ILiquidPoolFactory liquidPoolFactory,
    IExperienceSharingService experienceSharingService,
    IPlayerSkullService playerSkullService,
    GameConfiguration gameConfiguration): ICreatureDeathService
{
    public void Handle(ICombatActor deadCreature, IThing by, ILoot loot)
    {
        if (deadCreature is IMonster { IsSummon: true } summon)
        {
            //if summon just remove the creature from map
            map.RemoveCreature(summon);
            return;
        }

        ReplaceCreatureByCorpse(deadCreature, loot);
        CreateBlood(deadCreature);

        experienceSharingService.Share(deadCreature);

        var damageRecords = deadCreature.ReceivedDamages.GetDamageRecords(gameConfiguration.Death);
        
        ProcessDamageRecords(deadCreature, by, damageRecords);

        if (by is IPlayer aggressor)
        {
            playerSkullService.UpdatePlayerSkull(aggressor);
        }
    }

    private static void ProcessDamageRecords(ICombatActor deadCreature, IThing by, List<DamageRecord> damageRecords)
    {
        foreach (var damageRecord in damageRecords)
        {
            if (damageRecord.Aggressor is not ICombatActor damageOwner) return;

            if (by is ICombatActor aggressor && damageOwner.CreatureId == aggressor.CreatureId)
            {
                aggressor.Kill(deadCreature, true);
                continue;
            }
            
            damageOwner.Kill(deadCreature);
        }
    }

    private void ReplaceCreatureByCorpse(ICreature creature, ILoot loot)
    {
        var corpse = itemFactory.CreateLootCorpse(creature.CorpseType, creature.Location, loot);
        creature.Corpse = corpse;

        if (creature is IWalkableCreature walkable)
        {
            walkable.Tile.AddItem(corpse);
            map.RemoveCreature(creature);
        }

        corpse.Decay.StartDecay();
    }

    private void CreateBlood(ICreature creature)
    {
        if (creature is not ICombatActor victim) return;
        var liquidColor = victim.BloodType switch
        {
            BloodType.Blood => LiquidColor.Red,
            BloodType.Slime => LiquidColor.Green,
            _ => LiquidColor.Red
        };

        var pool = liquidPoolFactory.Create(victim.Location, liquidColor);

        map.CreateBloodPool(pool, victim.Tile);
    }
}