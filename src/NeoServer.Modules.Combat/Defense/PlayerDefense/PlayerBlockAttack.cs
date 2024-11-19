using NeoServer.Game.Common.Combat;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;

namespace NeoServer.Modules.Combat.Defense.PlayerDefense;

public static class PlayerBlockAttack
{
    /// <summary>
    ///     Attempt blocking an incoming attack
    /// </summary>
    public static CombatDamage TryBlock(ICombatActor defender, CombatDamage combatDamage)
    {
        if (defender is not IPlayer player) return combatDamage;

        // Attempt to block using immunity defense method
        ImmunityDefenseMethod.Defend(player, ref combatDamage);
        if (combatDamage.Damage <= 0)
        {
            player.UpdateBlockCounter(BlockType.Armor);
            return combatDamage;
        }

        // Attempt to block using shield defense method
        ShieldDefenseMethod.Defend(player, ref combatDamage);
        if (combatDamage.Damage <= 0)
        {
            player.IncreaseSkillCounter(SkillType.Shielding, 1);
            player.UpdateBlockCounter(BlockType.Shield);
            return combatDamage;
        }

        // Attempt to block using armor defense method
        ArmorDefenseMethod.Defend(player, ref combatDamage);
        if (combatDamage.Damage <= 0)
        {
            player.UpdateBlockCounter(BlockType.Armor);
            return combatDamage;
        }

        // Attempt to block using equipment defense method
        EquipmentDefenseMethod.Defend(player, ref combatDamage);
        if (combatDamage.Damage <= 0)
        {
            player.UpdateBlockCounter(BlockType.Armor);
            return combatDamage;
        }

        return combatDamage;
    }
}