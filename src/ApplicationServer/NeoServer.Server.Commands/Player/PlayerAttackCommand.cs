using NeoServer.Domain.Combat.Validations;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Services;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Commands;

namespace NeoServer.Server.Commands.Player;

public class PlayerAttackCommand(IGameCreatureManager gameCreatureManager, AttackValidation attackValidation) : ICommand
{
    public void Execute(IPlayer player, uint targetId)
    {
        if (targetId == 0)
        {
            player.StopAttack();
            return;
        }

        if (!gameCreatureManager.TryGetCreature(targetId, out var target)) return;

        var result = attackValidation.Validate(new AttackInput(player, target, new CombatParameter()));

        if (AttackValidation.ShouldStopAttackOnValidationFailure(result.Reason))
        {
            player.StopAttack(true);
            OperationFailService.Send(player, result.Reason);
            return;
        }

        player.SetAttackTarget(target);
    }
}