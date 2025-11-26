using NeoServer.Domain.Combat.Validations;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Services;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Common.Services;
using NeoServer.Domain.Creatures.Models.Bases;
using NeoServer.Domain.Creatures.Monster.Summon;
using Serilog;

namespace NeoServer.Domain.Combat.Attacks;

public class AttackService(
    ILogger logger,
    IPlayerSkullService playerSkullService,
    AreaAttackService areaAttackService,
    SingleTargetAttackService singleTargetAttackService,
    ConditionAttackService conditionAttackService,
    AttackValidation attackValidation) : IAttackService
{
    public CombatResult Execute(AttackInput attackInput)
    {
        if (Guard.IsNull(attackInput.Aggressor))
        {
            logger.Warning("Attack aggressor is null");
            return CombatResult.Fail(Result.NotPossible);
        }

        // Attack each combat actor on the target tile
        if (!attackInput.Parameters.IsAttackInArea &&
            attackInput.Target is IDynamicTile { Creatures.Count: > 0 } targetTile)
        {
            uint totalDamage = 0;
            var result = Result.NotPossible;

            foreach (var target in targetTile.Creatures.ToArray())
            {
                if (target is not ICombatActor) continue;
                var combatResult = Execute(new AttackInput(attackInput.Aggressor, target, attackInput.Parameters));

                totalDamage += combatResult.TotalDamage;

                if (result.Succeeded) continue;

                result = combatResult.Result;
            }

            return new CombatResult(totalDamage, result);
        }

        var attackValidationResult = attackValidation.Validate(attackInput);

        if (attackValidationResult.Failed)
        {
            var shouldStopAttack = AttackValidation.ShouldStopAttackOnValidationFailure(attackValidationResult.Reason);
            if (attackInput.Aggressor is CombatActor combatActor && shouldStopAttack)
            {
                combatActor.StopAttack();
            }

            if (attackInput.Aggressor is IPlayer player && shouldStopAttack)
            {
                OperationFailService.Send(player, attackValidationResult.Reason);
            }

            return CombatResult.Fail(attackValidationResult);
        }

        var pvpCombatValidationResult = attackValidation.ValidatePvpCombat(attackInput.Aggressor as IPlayer, attackInput.Target as ICombatActor);
        if (pvpCombatValidationResult.Failed) return CombatResult.Fail(pvpCombatValidationResult);

        // Update skull for direct player attacks or attacks on player summons
        var targetPlayer = attackInput.Target switch
        {
            IPlayer player => player,
            Summon { Master: IPlayer master } => master,
            _ => null
        };
        
        playerSkullService.UpdateSkullOnAttack(attackInput.Aggressor as IPlayer, targetPlayer);

        if (!DistanceAttackValidator.IsValid(attackInput))
            return CombatResult.Fail(Result.Fail(InvalidOperation.TooFar));

        UpdateParameters(attackInput);

        if (attackInput.Parameters.IsAttackInArea) return areaAttackService.Execute(attackInput);
        if (attackInput.Parameters.Conditions.Any()) return conditionAttackService.Execute(attackInput);

        return singleTargetAttackService.Execute(attackInput);
    }

    private static void UpdateParameters(AttackInput attackInput)
    {
        if (attackInput.Aggressor is not IPlayer playerAggressor) return;

        if (attackInput.Parameters.DamageFormula.Formula is FormulaType.MagicLevel)
        {
            var minMaxDamage = attackInput.Parameters.DamageFormula.Callback.Invoke(playerAggressor,
                playerAggressor.Skills[playerAggressor.SkillInUse].Level,
                playerAggressor.MagicLevel, 0);

            attackInput.Parameters.SetMinMaxDamage(minMaxDamage);
        }

        if (attackInput.Parameters.DamageFormula.Formula is FormulaType.Skill)
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