using System.Linq;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Conditions.Implementations;

namespace NeoServer.Server.Routines.Creatures;

public static class CreatureConditionRoutine
{
    public static void Execute(ICombatActor creature)
    {
        if (creature.IsDead) return;

        foreach (var condition in creature.GetConditions())
        {
            if (condition.HasExpired)
            {
                condition.End();
                creature.RemoveCondition(condition);
            }

            if (condition is ConditionDamage damageCondition) damageCondition.Execute(creature);
        }
    }
}