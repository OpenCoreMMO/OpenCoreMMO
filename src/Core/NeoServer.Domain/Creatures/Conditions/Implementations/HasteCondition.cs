using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Creatures.Structs;
using NeoServer.Domain.Creatures.Conditions.Enums;

namespace NeoServer.Domain.Creatures.Conditions.Implementations;

public class HasteCondition : BaseCondition
{
    public HasteCondition(
        uint interval,
        FormulaValues formulaValues,
        EffectT effect = EffectT.None) : base(interval)
    {
        Interval = interval;
        FormulaValues = formulaValues;
        Effect = effect;
    }

    public override ConditionType Type => ConditionType.Haste;
    public EffectT Effect { get; }
    public uint Interval { get; }

    internal override bool Start(ICreature creature)
    {
        if (!base.Start(creature))
            return false;

        if (creature is not IWalkableCreature walkableCreature)
            return false;
        
        //End any existing paralyze conditions
        if (creature is ICombatActor combatActor)
        {
            combatActor.RemoveCondition(ConditionType.Paralyze);
        }

        var baseSpeed = walkableCreature.RawSpeed;

        var min = walkableCreature.RawSpeed * FormulaValues.MinA + FormulaValues.MinB;
        var max = walkableCreature.RawSpeed * FormulaValues.MaxA + FormulaValues.MaxB;

        var random = Random.Shared;
        var randomSpeed = random.Next((int)min, (int)max);
        var speed = randomSpeed - baseSpeed;

        walkableCreature.IncreaseSpeed((ushort)speed);

        EndAction = () => walkableCreature.DecreaseSpeed((ushort)speed);

        return true;
    }
}