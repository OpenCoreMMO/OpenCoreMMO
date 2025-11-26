using NeoServer.Domain.Combat.Attacks;
using NeoServer.Domain.Combat.Validations;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Enums;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Common.Services;
using NeoServer.Domain.Creatures.Monster.Summon;
using NeoServer.Domain.Creatures.Player.Modes;

namespace NeoServer.Domain.Combat.Player;

public class PlayerCombatService(IAttackService attackService,  AttackValidation attackValidation, IPlayerSkullService playerSkullService)
{
    public void Attack(IPlayer player, ICombatActor target)
    {
        var combatParameter = PlayerCombatParameterBuilder.Build(player, target);

        var combatResult = attackService.Execute(new AttackInput(player, target, combatParameter));

        if (combatResult.Result.Failed) return;

        player.PostAttack(combatParameter, target, combatResult);
    }
    
    public void SetAttackTarget(IPlayer player, ICombatActor target)
    {
        if(Guard.AnyNull(player, target)) return;
        
        var result = attackValidation.Validate(new AttackInput(player, target, new CombatParameter()));

        if (AttackValidation.ShouldStopAttackOnValidationFailure(result.Reason))
        {
            player.StopAttack(true);
            OperationFailService.Send(player, result.Reason);
            return;
        }

        
        var pvpCombatValidationResult = attackValidation.ValidatePvpCombat(player, target);
        if (pvpCombatValidationResult.Failed) return;
        
        // Update skull for direct player attacks or attacks on player summons
        var targetPlayer = target switch
        {
            IPlayer playerTarget => playerTarget,
            Summon { Master: IPlayer master } => master,
            _ => null
        };
     
        player.SetAttackTarget(target);
        
        //side effects
        playerSkullService.UpdateSkullOnAttack(player, targetPlayer);
    }
    
   
}