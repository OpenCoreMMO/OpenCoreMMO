using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Creatures.Condition;

namespace NeoServer.Domain.Common.Parsers;

public static class ConditionTypeParser
{
    public static DamageType ToDamageType(this ConditionType type)
    {
        return type switch
        {
            ConditionType.Poisoned => DamageType.Earth,
            ConditionType.Burning => DamageType.FireField,
            ConditionType.Electrified => DamageType.Energy,
            ConditionType.Freezing => DamageType.Ice,
            ConditionType.Bleeding => DamageType.Physical,
            ConditionType.Drowning => DamageType.Drown,
            ConditionType.Cursed => DamageType.Death,
            ConditionType.Dazzled => DamageType.Holy,
            _ => DamageType.None
        };
    }

    public static ConditionType ToConditionType(this DamageType type)
    {
        return type switch
        {
            DamageType.Earth => ConditionType.Poisoned,
            DamageType.FireField => ConditionType.Burning,
            DamageType.Fire => ConditionType.Burning,
            DamageType.Energy => ConditionType.Electrified,
            _ => ConditionType.None
        };
    }

    public static ConditionType Parse(string type)
    {
        return type switch
        {
            "poison" => ConditionType.Poisoned,
            "fire" => ConditionType.Burning,
            "energy" => ConditionType.Electrified,
            "drunk" => ConditionType.Drunk,
            "drown" => ConditionType.Drowning,
            "curse" => ConditionType.Cursed,
            "freeze" => ConditionType.Freezing,
            "bleed" => ConditionType.Bleeding,
            "dazzle" => ConditionType.Dazzled,
            _ => ConditionType.None
        };
    }
}