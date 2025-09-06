using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Parsers;

namespace NeoServer.Domain.Creatures.Monster.Actions;

internal static class MonsterDefend
{
    public static int DefendUsingShield(Monster monster, int attack)
    {
        attack -= (ushort)GameRandom.Random.NextInRange(monster.Defense / 2f, monster.Defense);
        return attack;
    }

    public static int DefendUsingArmor(Monster monster, int attack)
    {
        switch (monster.ArmorRating)
        {
            case > 3:
                attack -= (ushort)GameRandom.Random.NextInRange(monster.ArmorRating / 2f,
                    monster.ArmorRating - (monster.ArmorRating % 2 + 1));
                break;
            case > 0:
                --attack;
                break;
        }

        return attack;
    }

    public static CombatDamage ImmunityDefend(Monster monster, CombatDamage damage)
    {
        if (damage.Damage <= 0) return damage;

        if (monster.HasImmunity(damage.Type.ToImmunity()))
        {
            damage.SetNewDamage(0);
            return damage;
        }

        if (monster.Metadata.ElementResistance is null) return damage;

        if (!monster.Metadata.ElementResistance.TryGetValue(damage.Type, out var resistance)) return damage;

        var valueToReduce = Math.Round(damage.Damage * (decimal)(resistance / 100f));

        damage.IncreaseDamage((int)-valueToReduce);

        return damage;
    }
}