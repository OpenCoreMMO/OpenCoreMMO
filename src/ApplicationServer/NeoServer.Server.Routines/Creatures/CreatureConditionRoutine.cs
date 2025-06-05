using System.Linq;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Condition;

namespace NeoServer.Server.Routines.Creatures;

public static class CreatureConditionRoutine
{
    public static void Execute(ICombatActor creature)
    {
        if (creature.IsDead) return;

        foreach (var (_, condition) in creature.Conditions.ToList())
        {
            if (condition.HasExpired)
            {
                condition.End();
                creature.RemoveCondition(condition);
            }

            if (condition is DamageCondition damageCondition) damageCondition.Execute(creature);
        }
    }
}