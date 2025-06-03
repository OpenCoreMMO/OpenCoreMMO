using NeoServer.Domain.Common.Item;
using NeoServer.Scripts.LuaJIT.Models.Combat;

namespace NeoServer.Scripts.LuaJIT.Parsers;

public static class CombatTypeParser
{
    public static DamageType ToDamageType(this CombatType combatType)
    {
        return combatType switch
        {
            CombatType.COMBAT_PHYSICALDAMAGE => DamageType.Physical,
            CombatType.COMBAT_ENERGYDAMAGE => DamageType.Energy,
            CombatType.COMBAT_EARTHDAMAGE => DamageType.Earth,
            CombatType.COMBAT_FIREDAMAGE => DamageType.Fire,
            CombatType.COMBAT_UNDEFINEDDAMAGE => DamageType.None,
            CombatType.COMBAT_LIFEDRAIN => DamageType.LifeDrain,
            CombatType.COMBAT_MANADRAIN => DamageType.ManaDrain,
            CombatType.COMBAT_HEALING => DamageType.None, // Not damage
            CombatType.COMBAT_DROWNDAMAGE => DamageType.Drown,
            CombatType.COMBAT_ICEDAMAGE => DamageType.Ice,
            CombatType.COMBAT_HOLYDAMAGE => DamageType.Holy,
            CombatType.COMBAT_DEATHDAMAGE => DamageType.Death,
            CombatType.COMBAT_AGONYDAMAGE => DamageType.None, // No direct match; interpreted
            CombatType.COMBAT_NEUTRALDAMAGE => DamageType.None, // No direct match; interpreted

            CombatType.COMBAT_NONE => DamageType.None,

            _ => DamageType.None
        };
    }
}