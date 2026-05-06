using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Conditions.Enums;

namespace NeoServer.Domain.Creatures.Conditions.Implementations;

public class ParalyzeCondition : BaseCondition
{
    private readonly ushort _speedReduction;

    public override ConditionType Type => ConditionType.Paralyze;

    public ParalyzeCondition(uint duration, ushort speedReduction) : base(duration)
    {
        _speedReduction = speedReduction;
    }

    internal override bool Start(ICreature creature)
    {
        if (!base.Start(creature)) return false;

        if (creature is not IWalkableCreature walkable) return true;
        
        if (creature is ICombatActor combatActor)
        {
            combatActor.RemoveCondition(ConditionType.Haste);
        }

        walkable.DecreaseSpeed(_speedReduction);

        EndAction = () => walkable.IncreaseSpeed(_speedReduction);
        
        return true;
    }
}
