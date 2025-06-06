using NeoServer.Domain.Creatures.Condition;

namespace NeoServer.Domain.Common.Parsers;

public static class ConditionIconParser
{
    public static ConditionIcon Parse(ConditionType type)
    {
        return type switch
        {
            ConditionType.Haste => ConditionIcon.Haste,
            ConditionType.Poisoned => ConditionIcon.Poison,
            ConditionType.LogoutBlock => ConditionIcon.Swords,
            ConditionType.Paralyze => ConditionIcon.Paralyze,
            ConditionType.Burning => ConditionIcon.Burn,
            ConditionType.Electrified => ConditionIcon.Energy,
            ConditionType.Drunk => ConditionIcon.Drunk,
            ConditionType.Cursed => ConditionIcon.Cursed,
            ConditionType.Freezing => ConditionIcon.Freezing,
            ConditionType.ManaShield => ConditionIcon.ManaShield,
            ConditionType.Drowning => ConditionIcon.Drowning,
            ConditionType.Pacified => ConditionIcon.Pigeon,
            ConditionType.ProtectionZoneBlock => ConditionIcon.RedSwords,
            _ => ConditionIcon.None
        };
    }
}