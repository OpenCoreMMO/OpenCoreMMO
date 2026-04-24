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
    public uint ColorLevel { get; private set; }
    public uint Color { get; }
    public uint InternalLightTicks { get; private set; }
    public uint LightChangeInterval { get; set; }

    public override bool Start(ICreature creature)
    {
        if (!base.Start(creature))
            return false;

        InternalLightTicks = 0;

        var previousLight = creature.LightLevel;

        if (creature is ICombatActor combatActor)
        {
            //End existing light conditions
            combatActor.Conditions.EndConditions(ConditionType.Light);
        }

        ColorLevel = Math.Max(previousLight, ColorLevel);
        LightChangeInterval = Duration == 0 || ColorLevel == 0
            ? 0
            : (uint)(Duration / TimeSpan.TicksPerMillisecond) / ColorLevel;
        creature.SetLight((byte)Color, (byte)ColorLevel);

        EndAction = creature.RemoveLight;

        return true;
    }

    public void Execute(ICreature creature, int interval)
    {
        if (ColorLevel == 0 || creature.LightLevel == 0)
            return;

        InternalLightTicks += (uint)Math.Max(0, interval);

        if (InternalLightTicks < LightChangeInterval)
            return;

        InternalLightTicks = 0;

        if (creature.LightLevel == 0)
            return;

        creature.SetLight((byte)Color, (byte)(creature.LightLevel - 1));
    }
}
