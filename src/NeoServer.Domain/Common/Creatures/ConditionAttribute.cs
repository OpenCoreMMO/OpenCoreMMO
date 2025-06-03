namespace NeoServer.Domain.Common.Creatures;

public enum ConditionAttribute
{
    Type = 1,
    Id,
    Ticks,
    HealthTicks,
    HealthGain,
    ManaTicks,
    ManaGain,
    Delayed,
    Owner,
    IntervalData,
    SpeedDelta,
    FormulaMinA,
    FormulaMinB,
    FormulaMaxA,
    FormulaMaxB,
    LightColor,
    LightLevel,
    LightTicks,
    LightInterval,
    SoulTicks,
    SoulGain,
    Skills,
    Stats,
    Outfit,
    PeriodDamage,
    IsBuff,
    SubId,
    End = 254 //reserved for serialization
}