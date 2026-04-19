using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Creatures.Conditions.Enums;

namespace NeoServer.Domain.Creatures.Conditions.Implementations;

public class ConditionLight : BaseCondition
{
    public ConditionLight(
        uint interval,
        uint lightLevel,
        uint lightColor,
        EffectT effect = EffectT.None) : base(interval)
    {
        ColorLevel = lightLevel;
        Color = lightColor;
        Effect = effect;
    }

    public override ConditionType Type => ConditionType.Light;
    public EffectT Effect { get; }
    public uint ColorLevel { get; }
    public uint Color { get; }
    public uint InternalLightTicks { get; private set; }
    public uint LightChangeInterval { get; set; }

    public override bool Start(ICreature creature)
    {
        if (!base.Start(creature))
            return false;

        InternalLightTicks = 0;
        LightChangeInterval = (uint)Duration / ColorLevel;
        creature.SetLight((byte)Color, (byte)ColorLevel);

        EndAction = creature.RemoveLight;

        return true;
    }
}