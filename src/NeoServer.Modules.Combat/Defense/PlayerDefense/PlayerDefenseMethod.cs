using NeoServer.Game.Common.Combat;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Parsers;

namespace NeoServer.Modules.Combat.Defense.PlayerDefense;

public static class ShieldDefenseMethod
{
    public static void Defend(IPlayer player, ref CombatDamage combatDamage)
    {
        if (combatDamage.Damage <= 0) return;
        if (!player.CanBlock(combatDamage)) return;
        var defense = player.Inventory.TotalDefense * player.Skills[SkillType.Shielding].Level *
                      (player.DefenseFactor / 100d) -
                      combatDamage.Damage / 100d * player.ArmorRating * (player.Vocation.Formula?.Defense ?? 1f);

        var resultDamage = (int)(combatDamage.Damage - defense);
        //if (resultDamage <= 0) player.IncreaseSkillCounter(SkillType.Shielding, 1);
        combatDamage.SetNewDamage(resultDamage);
    }
}

public static class ArmorDefenseMethod
{
    public static void Defend(IPlayer player, ref CombatDamage combatDamage)
    {
        if (combatDamage.Damage <= 0) return;
        if (combatDamage.IsElementalDamage) return;
        int damage = combatDamage.Damage;
        switch (player.ArmorRating)
        {
            case > 3:
            {
                var min = player.ArmorRating / 2 * (player.Vocation.Formula?.Armor ?? 1f);
                var max = (player.ArmorRating / 2 * 2 - 1) * (player.Vocation.Formula?.Armor ?? 1f);
                damage -= (ushort)GameRandom.Random.NextInRange(min, max);
                break;
            }
            case > 0:
                --damage;
                break;
        }

        combatDamage.SetNewDamage(damage);
    }
}

public static class ImmunityDefenseMethod
{
    public static void Defend(IPlayer player, ref CombatDamage combatDamage)
    {
        if (combatDamage.Damage <= 0) return;

        if (!player.HasImmunity(combatDamage.Type.ToImmunity())) return;
        combatDamage.SetNewDamage(0);
    }
}

public static class EquipmentDefenseMethod
{
    public static void Defend(IPlayer player, ref CombatDamage combatDamage)
    {
        if (combatDamage.Damage <= 0) return;

        foreach (var equipment in player.Inventory.DressingEquipments)
        {
            if (equipment is not IProtection protection) continue;

            protection.Protect(ref combatDamage);
        }
    }
}