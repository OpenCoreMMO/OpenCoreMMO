using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Creatures.Structs;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Creatures.Conditions.Enums;

namespace NeoServer.Domain.Creatures.Conditions.Implementations;

public class ConditionLight : BaseCondition
{
    private CooldownTime _cooldown;

    public ConditionLight(ConditionType type, uint interval, uint lightLevel, uint lightColor,
        EffectT effect = EffectT.None) : base(interval)
    {
        Type = type;
        Interval = interval;
        ColorLevel = lightLevel;
        Color = lightColor;
        Effect = effect;
    }

    public byte Amount { get; }
    public override ConditionType Type { get; }
    public DamageType DamageType { get; set; }
    public EffectT Effect { get; }

    public uint Interval
    {
        set => _cooldown = new CooldownTime(DateTime.Now, value);
    }

    public uint ColorLevel { get; private set; } = 0;
    public uint Color { get; private set; } = 215;
    public uint InternalLightTicks { get; private set; } = 0;
    public uint LightChangeInterval { get; set; } = 0;

    public override bool Start(ICreature creature)
    {
        if (!base.Start(creature))
            return false;

        InternalLightTicks = 0;
        LightChangeInterval = (uint)Duration / ColorLevel;
        creature.SetLight((byte)Color, (byte)ColorLevel);

        return true;
    }

}