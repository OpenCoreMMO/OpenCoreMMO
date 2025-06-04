using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Creatures.Condition;

namespace NeoServer.Domain.Common.Parsers;

public class ConditionTypeParser
{
    public static DamageType Parse(ConditionType type)
    {
        return type switch
        {
            ConditionType.Poison => DamageType.Earth,
            ConditionType.Fire => DamageType.FireField,
            ConditionType.Energy => DamageType.Energy,
            _ => DamageType.None
        };
    }

    public static ConditionType Parse(DamageType type)
    {
        return type switch
        {
            DamageType.Earth => ConditionType.Poison,
            DamageType.FireField => ConditionType.Fire,
            DamageType.Fire => ConditionType.Fire,
            DamageType.Energy => ConditionType.Energy,
            _ => ConditionType.None
        };
    }
}