using NeoServer.Domain.Combat.Attacks;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Combat.Player;

public class PlayerCombatService(IAttackService attackService)
{
    public void Attack(IPlayer player, ICombatActor target)
    {
        var combatParameter = PlayerCombatParameterBuilder.Build(player, target);
        
        attackService.Execute(new AttackInput(player, target, combatParameter));
    }
}