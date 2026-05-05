using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Conditions.Enums;

namespace NeoServer.Domain.Creatures.Conditions.Implementations;

public class PacifiedCondition : BaseCondition
{
    public override ConditionType Type => ConditionType.Pacified;

    public PacifiedCondition() : base(0)
    {
    }

    internal override bool Start(ICreature creature)
    {
        if (!base.Start(creature)) return false;

        if (creature is ICombatActor combatActor)
        {
            combatActor.RemoveCondition(ConditionType.LogoutBlock);
            combatActor.RemoveCondition(ConditionType.ProtectionZoneBlock);
        }

        return true;
    }
}
