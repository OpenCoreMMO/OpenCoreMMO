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
    IExperienceSharingService experienceSharingService): ICreatureDeathService
{
    public void Handle(ICombatActor deadCreature, IThing by, ILoot loot)
    {
        if (deadCreature is IMonster { IsSummon: true } summon)
        {
            //if summon just remove the creature from map
            map.RemoveCreature(summon);
            return;
        }

        ReplaceCreatureByCorpse(deadCreature, by, loot);
        CreateBlood(deadCreature);

        experienceSharingService.Share(deadCreature);

        if (by is ICombatActor aggressor) aggressor.Kill(deadCreature);
    }

    private void ReplaceCreatureByCorpse(ICreature creature, IThing by, ILoot loot)
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