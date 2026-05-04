using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Conditions.Enums;

namespace NeoServer.Domain.Creatures.Conditions.Implementations;

public class CombatBlockCondition : BaseCondition
{
    private readonly ConditionType _type;

    public override ConditionType Type => _type;

    public CombatBlockCondition(ConditionType type) : base(0)
    {
        _type = type;
    }

    public override bool Start(ICreature creature)
    {
        if (!base.Start(creature)) return false;

        if (creature is ICombatActor combatActor)
        {
            combatActor.RemoveCondition(ConditionType.Pacified);
            combatActor.RemoveCondition(_type);
        }

        return true;
    }
}
