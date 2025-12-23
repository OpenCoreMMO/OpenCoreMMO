using NeoServer.Domain.Common.Combat;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Creatures.Monster.Loot;
using NeoServer.Domain.Creatures.Monster.Summon;
using NeoServer.Domain.Services;

namespace NeoServer.Domain.Creatures.Services;

public class CreatureDeathService(
    IItemFactory itemFactory,
    IMap map,
    BloodPoolService bloodPoolService) : ICreatureDeathService
{
    public void Handle(ICombatActor deadCreature, IThing by, List<DamageRecord> damageRecords)
    {
        if (deadCreature is IMonster { IsSummon: true }) //do not create blood or corpse for summons
            return;

        //do not create blood or corpse for monsters that are killed by another monster
        if (deadCreature is IMonster && by is IMonster and not Summon { Master: IPlayer }) return;

        bloodPoolService.CreatePool(deadCreature);
        ReplaceCreatureByCorpse(deadCreature, by);

        ProcessDamageRecords(deadCreature, by, damageRecords);
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

    private void ReplaceCreatureByCorpse(ICreature creature, IThing killer)
    {
        creature.Corpse ??= itemFactory.CreateLootCorpse(creature.CorpseType, creature.Location, new Loot([]), killer);

        if (creature.Corpse is not IItem corpse) return;

        if (creature is IWalkableCreature walkable)
        {
            walkable.Tile.AddItem(corpse);
            map.RemoveCreature(creature);
        }

        corpse.Decay?.StartDecay();
    }
}