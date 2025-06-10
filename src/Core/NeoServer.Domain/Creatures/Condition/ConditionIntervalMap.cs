namespace NeoServer.Domain.Creatures.Condition;

public static class ConditionIntervalMap
{
    public static uint Get(ConditionType condition)
    {
        return condition switch
        {
            ConditionType.Poisoned => 4_000,
            ConditionType.Burning => 9_000,
            ConditionType.Electrified => 11_000,
            ConditionType.Drowning => 4_000,
            ConditionType.Cursed => 4_000,
            ConditionType.Freezing => 2_000,
            ConditionType.Bleeding => 4_000,
            ConditionType.Dazzled => 4_000,
            _ => 0
        };
    }
}