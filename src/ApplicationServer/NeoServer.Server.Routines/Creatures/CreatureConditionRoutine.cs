using System.Linq;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Conditions.Implementations;

namespace NeoServer.Server.Routines.Creatures;

public static class CreatureConditionRoutine
{
    public static void Execute(ICombatActor creature, int interval)
    {
        if (creature.IsDead) return;
        var conditions = creature.GetConditions();
        for (var i = 0; i < conditions.Count; i++)
        {
            var condition = conditions[i]; 

            if (condition.HasExpired)
            {
                creature.RemoveCondition(condition);
            }

            if (condition is ConditionLight lightCondition)
            {
                lightCondition.Execute(creature, interval);
            }

            if (condition is ConditionDamage damageCondition)
            {
                damageCondition.Execute(creature);
            }
        }
    }
}
