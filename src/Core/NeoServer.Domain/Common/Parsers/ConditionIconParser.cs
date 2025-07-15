using NeoServer.Domain.Creatures.Conditions.Enums;

namespace NeoServer.Domain.Common.Parsers;

public static class ConditionIconParser
{
    public static ConditionIconType Parse(ConditionType type)
    {
        return type switch
        {
            ConditionType.Haste => ConditionIconType.Haste,
            ConditionType.Poisoned => ConditionIconType.Poison,
            ConditionType.LogoutBlock => ConditionIconType.Swords,
            ConditionType.Paralyze => ConditionIconType.Paralyze,
            ConditionType.Burning => ConditionIconType.Burn,
            ConditionType.Electrified => ConditionIconType.Energy,
            ConditionType.Drunk => ConditionIconType.Drunk,
            ConditionType.Cursed => ConditionIconType.Cursed,
            ConditionType.Freezing => ConditionIconType.Freezing,
            ConditionType.ManaShield => ConditionIconType.ManaShield,
            ConditionType.Drowning => ConditionIconType.Drowning,
            ConditionType.Pacified => ConditionIconType.Pigeon,
            ConditionType.ProtectionZoneBlock => ConditionIconType.RedSwords,
            _ => ConditionIconType.None
        };
    }
}