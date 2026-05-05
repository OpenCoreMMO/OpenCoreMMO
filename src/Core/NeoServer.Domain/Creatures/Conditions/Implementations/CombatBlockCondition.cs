using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Conditions.Enums;

namespace NeoServer.Domain.Creatures.Conditions.Implementations;

public class CombatBlockCondition(ConditionType type) : BaseCondition(0)
{
    public override ConditionType Type => type;

    internal override bool Start(ICreature creature)
    {
        if (!base.Start(creature)) return false;

        if (creature is ICombatActor combatActor)
        {
            combatActor.RemoveCondition(ConditionType.Pacified);
        }

        return true;
    }
}
