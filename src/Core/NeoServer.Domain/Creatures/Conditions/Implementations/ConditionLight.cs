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
    
    public byte CurrentColorLevel { get; private set; }
    public uint Color { get; }
    public uint LightChangeInterval { get; set; }
    
    private uint _lightTicksInterval;

    public override bool Start(ICreature creature)
    {
        if (!base.Start(creature))
            return false;
        
        _lightTicksInterval = 0;
        LightChangeInterval = Duration == 0
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

        _lightTicksInterval += (uint)Math.Max(0, interval);

        if (_lightTicksInterval < LightChangeInterval)
            return;

        _lightTicksInterval = 0;

        if (creature.LightLevel == 0)
            return;

        CurrentColorLevel = (byte)(creature.LightLevel - 1);

        creature.SetLight((byte)Color, CurrentColorLevel);
    }
}
