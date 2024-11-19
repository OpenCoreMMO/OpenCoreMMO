using NeoServer.Game.Common.Combat;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Parsers;

namespace NeoServer.Modules.Combat.Defense.MonsterDefense;

public static class ShieldDefenseMethod
{
    public static void Defend(IMonster monster, ref CombatDamage combatDamage)
    {
        if (!monster.CanBlock(combatDamage)) return;

        int damage = combatDamage.Damage;
        damage -= (int)GameRandom.Random.NextInRange(monster.Defense / 2f, monster.Defense);
        combatDamage.SetNewDamage(damage);
    }
}

public static class ArmorDefenseMethod
{
    public static void Defend(IMonster monster, ref CombatDamage combatDamage)
    {
        if (combatDamage.IsElementalDamage) return;

        int damage = combatDamage.Damage;
        switch (monster.ArmorRating)
        {
            case > 3:
                damage -= (ushort)GameRandom.Random.NextInRange(monster.ArmorRating / 2f,
                    monster.ArmorRating - (monster.ArmorRating % 2 + 1));
                break;
            case > 0:
                --damage;
                break;
        }

        combatDamage.SetNewDamage(damage);
    }
}

public static class ImmunityDefenseMethod
{
    public static void Defend(IMonster monster, ref CombatDamage combatDamage)
    {
        if (combatDamage.Damage <= 0) return;

        if (monster.HasImmunity(combatDamage.Type.ToImmunity())) combatDamage.SetNewDamage(0);
    }
}

public static class ElementalDefenseMethod
{
    public static void Defend(IMonster monster, ref CombatDamage combatDamage)
    {
        if (!combatDamage.IsElementalDamage) return;
        if (combatDamage.Damage <= 0) return;

        if (monster.Metadata.ElementResistance is null) return;

        if (!monster.Metadata.ElementResistance.TryGetValue(combatDamage.Type, out var resistance)) return;

        combatDamage.ReduceDamageByPercent(resistance);
    }
}