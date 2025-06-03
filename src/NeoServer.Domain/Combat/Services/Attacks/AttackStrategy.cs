using NeoServer.Domain.Common.Combat.Structs;

namespace NeoServer.Domain.Combat.Services.Attacks;

public class AttackStrategy(SingleTargetAttackService singleTargetAttackService, AreaAttackService areaAttackService)
{
    public IAttackService GetAttackService(CombatParameter combatParameter)
    {
        if (combatParameter.IsAttackInArea) return areaAttackService;

        return singleTargetAttackService;
    }
}