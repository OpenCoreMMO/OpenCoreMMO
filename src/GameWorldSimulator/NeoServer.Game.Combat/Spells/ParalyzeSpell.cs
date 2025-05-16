using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;

namespace NeoServer.Game.Combat.Spells;

public class ParalyzeSpell : Spell<ParalyzeSpell>
{
    private float MaxA;
    private float MaxB;

    private float MinA;
    private float MinB;
    public override string Name => "Paralyze";
    public override EffectT Effect => EffectT.GlitterRed;
    public virtual ushort SpeedChange => 200;
    public override uint Duration => 10000;
    public override ushort Mana => 60;
    public override ConditionType ConditionType => ConditionType.Paralyze;

    public override bool OnCast(ICombatActor caster, string words, out InvalidOperation error)
    {
        error = InvalidOperation.None;

        var min = caster.Speed * MinA + MinB;
        var max = caster.Speed * MaxA + MaxB;

        caster.DecreaseSpeed(SpeedChange);
        return true;
    }

    public override void OnEnd(ICombatActor actor)
    {
        actor.IncreaseSpeed(SpeedChange);
        base.OnEnd(actor);
    }

    public void SetFormula(float minA, float minB, float maxA, float maxB)
    {
        MinA = minA;
        MinB = minB;
        MaxA = maxA;
        MaxB = maxB;
    }
}