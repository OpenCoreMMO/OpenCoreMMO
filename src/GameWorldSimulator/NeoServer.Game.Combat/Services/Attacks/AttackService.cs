using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Enums;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Services;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Results;
using NeoServer.Game.Common.Services;
using Serilog;

namespace NeoServer.Game.Combat.Services.Attacks;

public class AttackService(
    ILogger logger,
    PvPConfiguration pvpConfiguration,
    IPlayerSkullService playerSkullService,
    AttackStrategy attackStrategy,
    AttackValidation attackValidation) : IAttackService
{
    public Result Execute(AttackInput attackInput)
    {
        if (!HasValidInput(attackInput)) return Result.NotPossible;

        var attackValidationResult = attackValidation.Validate(attackInput);
        if (attackValidationResult.Failed) return attackValidationResult;

        var pvpCombatValidationResult = ValidatePvpCombat(attackInput);
        if (pvpCombatValidationResult.Failed) return pvpCombatValidationResult;

        playerSkullService.UpdateSkullOnAttack(attackInput.Aggressor as IPlayer, attackInput.Target as IPlayer);

        return attackStrategy.GetAttackService(attackInput.Parameters.Type).Execute(attackInput);
    }

    private bool HasValidInput(AttackInput attackInput)
    {
        if (Guard.IsNull(attackInput.Target))
        {
            logger.Warning("Attack target is null");
            return false;
        }

        if (Guard.IsNull(attackInput.Aggressor))
        {
            logger.Warning("Attack aggressor is null");
            return false;
        }

        if (attackInput.Parameters.Type is AttackType.None)
        {
            logger.Warning("Attack type is none");
            return false;
        }

        return true;
    }

    private Result ValidatePvpCombat(AttackInput attackInput)
    {
        if (attackInput.Aggressor is not IPlayer playerAggressor ||
            attackInput.Target is not IPlayer playerTarget)
        {
            //not pvp combat
            return Result.Success;
        }

        //pvp combat is not allowed in optional pvp
        if (pvpConfiguration.PvpMode == "Optional")
        {
            //todo: error message
            playerAggressor.StopAttack();
            OperationFailService.Send(playerAggressor,
                "You cannot attack other person.");
            return Result.NotPossible;
        }

        var targetHasSkull = playerTarget?.GetSkull(playerAggressor) is not Skull.None;

        if (!targetHasSkull && playerAggressor.SecureMode is PvpSecureMode.PvPDisabled &&
            attackInput.Parameters.NeedTarget)
        {
            playerAggressor.StopAttack(true);
            OperationFailService.Send(playerAggressor,
                InvalidOperation.AdjustCombatSettingsToAttackPlayer);
            return Result.Fail(InvalidOperation.AdjustCombatSettingsToAttackPlayer);
        }

        return Result.Success;
    }
}