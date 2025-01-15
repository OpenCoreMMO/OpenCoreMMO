using NeoServer.Game.Common.Creatures;

namespace NeoServer.Game.Common.Parsers;

public static class ConditionIconParser
{
    public static ConditionIcon Parse(ConditionType type)
    {
        return type switch
        {
            ConditionType.Haste => ConditionIcon.Haste,
            ConditionType.Poison => ConditionIcon.Poison,
            ConditionType.LogoutBlock => ConditionIcon.Swords,
            ConditionType.Paralyze => ConditionIcon.Paralyze,
            ConditionType.Fire => ConditionIcon.Burn,
            ConditionType.Energy => ConditionIcon.Energy,
            ConditionType.Drunk => ConditionIcon.Drunk,
            ConditionType.Cursed => ConditionIcon.Cursed,
            ConditionType.Freezing => ConditionIcon.Freezing,
            ConditionType.Manashield => ConditionIcon.ManaShield,
            ConditionType.Drown => ConditionIcon.Drowning,
            ConditionType.Pacified => ConditionIcon.Pigeon,
            _ => ConditionIcon.None
        };
    }
}