using NeoServer.Domain.Combat.Services.Attacks;
using NeoServer.Domain.Combat.Validations;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Enums;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Common.Services;
using NeoServer.Domain.Creatures.Player.Modes;
using Serilog;

namespace NeoServer.Domain.Combat.Attacks;

public class AttackService(
    ILogger logger,
    IPlayerSkullService playerSkullService,
    AreaAttackService areaAttackService,
    SingleTargetAttackService singleTargetAttackService,
    AttackValidation attackValidation) : IAttackService
{
    public Result Execute(AttackInput attackInput)
    {
        if (Guard.IsNull(attackInput.Aggressor))
        {
            logger.Warning("Attack aggressor is null");
            return Result.NotPossible;
        }

        // Attack each combat actor on the target tile
        if (!attackInput.Parameters.IsAttackInArea &&
            attackInput.Target is IDynamicTile { Creatures.Count: > 0 } targetTile)
        {
            foreach (var target in targetTile.Creatures)
            {
                if (target is not ICombatActor) continue;
                Execute(new AttackInput(attackInput.Aggressor, target, attackInput.Parameters));
            }

            return Result.Success;
        }

        var attackValidationResult = attackValidation.Validate(attackInput);
        if (attackValidationResult.Failed) return attackValidationResult;

        var pvpCombatValidationResult = ValidatePvpCombat(attackInput);
        if (pvpCombatValidationResult.Failed) return pvpCombatValidationResult;

        playerSkullService.UpdateSkullOnAttack(attackInput.Aggressor as IPlayer, attackInput.Target as IPlayer);

        if (DistanceAttackValidator.IsValid(attackInput) == false) return Result.Fail(InvalidOperation.TooFar);

        UpdateParameters(attackInput);

        if (attackInput.Parameters.IsAttackInArea) return areaAttackService.Execute(attackInput);

        return singleTargetAttackService.Execute(attackInput);
    }

    private static void UpdateParameters(AttackInput attackInput)
    {
        if (attackInput.Aggressor is not IPlayer playerAggressor) return;

        if (attackInput.Parameters.DamageFormula.Formula is CombatFormula.MagicLevel)
        {
            var minMaxDamage = attackInput.Parameters.DamageFormula.Callback.Invoke(playerAggressor,
                playerAggressor.Skills[playerAggressor.SkillInUse].Level,
                playerAggressor.MagicLevel, 0);

            attackInput.Parameters.SetMinMaxDamage(minMaxDamage);
        }

        if (attackInput.Parameters.DamageFormula.Formula is CombatFormula.Skill)
        {
            var minMaxDamage = attackInput.Parameters.DamageFormula.Callback.Invoke(playerAggressor,
                playerAggressor.Skills[playerAggressor.SkillInUse].Level,
                playerAggressor.Inventory.TotalAttack, (decimal)playerAggressor.DamageFactor);

            attackInput.Parameters.SetMinMaxDamage(minMaxDamage);

            var extraAttack = CalculateElementalAttack(playerAggressor);
            attackInput.Parameters.SetExtraAttack(extraAttack);
        }

        if (attackInput.Parameters.ShootType == ShootType.WeaponType)
            attackInput.Parameters.ShootType = playerAggressor.SkillInUse switch
            {
                SkillType.Axe => ShootType.WhirlwindAxe,
                SkillType.Club => ShootType.WhirlwindClub,
                SkillType.Sword => ShootType.WhirlwindSword,
                _ => ShootType.None
            };
    }

    private Result ValidatePvpCombat(AttackInput attackInput)
    {
        if (Equals(attackInput.Aggressor, attackInput.Target)) return Result.Success;
        if (attackInput.Aggressor is not IPlayer playerAggressor ||
            attackInput.Target is not IPlayer playerTarget)
            //not pvp combat
            return Result.Success;

        //pvp combat is not allowed in optional pvp
        // if (pvpConfiguration.PvpMode == "Optional")
        // {
        //     //todo: error message
        //     playerAggressor.StopAttack();
        //     OperationFailService.Send(playerAggressor,
        //         "You cannot attack other person.");
        //     return Result.NotPossible;
        // }

        var targetHasSkull = playerTarget?.GetSkull(playerAggressor) is not Skull.None;

        var tryingToAttackWithPvpDisabled = !targetHasSkull && playerAggressor.SecureMode is PvpSecureMode.PvPDisabled;

        if (tryingToAttackWithPvpDisabled)
        {
            playerAggressor.StopAttack(true);
            OperationFailService.Send(playerAggressor,
                InvalidOperation.AdjustCombatSettingsToAttackPlayer);
            return Result.Fail(InvalidOperation.AdjustCombatSettingsToAttackPlayer);
        }

        return Result.Success;
    }

    private static ExtraAttack CalculateElementalAttack(ICombatActor aggressor)
    {
        if (aggressor.MaximumElementalAttackPower is 0) return default;

        if (aggressor is not IPlayer player) return default;

        var damageType = player.Inventory.TotalElementalAttack.DamageType;

        if (damageType == DamageType.None) return default;

        return new ExtraAttack
        {
            MinDamage = aggressor.MinimumAttackPower,
            MaxDamage = aggressor.MaximumElementalAttackPower,
            DamageType = damageType
        };
    }
}