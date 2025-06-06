using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Creatures.Condition;

namespace NeoServer.Domain.Common.Parsers;

public class ConditionTypeParser
{
    public static DamageType Parse(ConditionType type)
    {
        return type switch
        {
            ConditionType.Poisoned => DamageType.Earth,
            ConditionType.Burning => DamageType.FireField,
            ConditionType.Electrified => DamageType.Energy,
            _ => DamageType.None
        };
    }

    public static ConditionType Parse(DamageType type)
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
            _ => ConditionType.None
        };
    }
}