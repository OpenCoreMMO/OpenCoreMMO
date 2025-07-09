using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using NeoServer.Scripts.LuaJIT.Models.Combat;

namespace NeoServer.Scripts.LuaJIT.Services;

public class NonAggressiveCombatService
{
    public void Execute(LuaCombat combat, ICreature caster, IThing target)
    {
        target ??= caster;

        if (target is ICombatActor targetCreature)
        {
            AddConditions(combat, targetCreature);
        }
    }

    private static void AddConditions(LuaCombat combat, ICombatActor targetCreature)
    {
        foreach (var condition in combat.Conditions)
        {
            condition.Parameters.TryGetValue(ConditionParamType.Ticks, out var duration);
                
            if (condition.Type is ConditionType.Haste)
            {
                targetCreature.AddCondition(new ConditionSpeed(condition.Type, duration, condition.FormulaValues));
            }
            
            if (condition.Type is ConditionType.Light)
            {
                condition.Parameters.TryGetValue(ConditionParamType.LightLevel, out var lightLevel);
                condition.Parameters.TryGetValue(ConditionParamType.LightColor, out var lightColor);

                targetCreature.AddCondition(new ConditionLight(duration, lightLevel, lightColor));
            }
        }
    }
}