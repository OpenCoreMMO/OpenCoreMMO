using NeoServer.Game.Common.Combat;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Modules.Combat.Defense.MonsterDefense;

public static class MonsterBlockAttack
{
    public static CombatDamage TryBlock(ICombatActor defender, CombatDamage combatDamage)
    {
        if (defender is not IMonster monster) return combatDamage;

        if (combatDamage.Damage <= 0) return combatDamage;

        ImmunityDefenseMethod.Defend(monster, ref combatDamage);
        if (combatDamage.Damage <= 0)
        {
            monster.UpdateBlockCounter(BlockType.Armor);
            return combatDamage;
        }

        ShieldDefenseMethod.Defend(monster, ref combatDamage);
        if (combatDamage.Damage <= 0)
        {
            monster.UpdateBlockCounter(BlockType.Shield);
            return combatDamage;
        }

        ArmorDefenseMethod.Defend(monster, ref combatDamage);
        if (combatDamage.Damage <= 0)
        {
            monster.UpdateBlockCounter(BlockType.Armor);
            return combatDamage;
        }

        ElementalDefenseMethod.Defend(monster, ref combatDamage);
        if (combatDamage.Damage <= 0)
        {
            monster.UpdateBlockCounter(BlockType.Armor);
            return combatDamage;
        }

        return combatDamage;
    }
}