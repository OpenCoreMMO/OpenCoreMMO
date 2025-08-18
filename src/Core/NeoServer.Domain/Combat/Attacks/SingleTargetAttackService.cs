using NeoServer.Domain.Combat.Calculations;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Enums;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Domain.Services;

namespace NeoServer.Domain.Combat.Attacks;

public class SingleTargetAttackService(
    IEventAggregator eventAggregator,
    CombatConfiguration combatConfiguration,
    ConditionAttackService conditionAttackService,
    MagicFieldService magicFieldService)
    : IAttackService
{
    public CombatResult Execute(AttackInput attackInput)
    {
        var aggressor = attackInput.Aggressor as ICombatActor;
        var target = attackInput.Target ?? aggressor?.CurrentTarget;

        if (IsAttackMissed(attackInput))
        {
            PublishAttackEvent(attackInput, true);
            aggressor?.PreAttack(CreateCombatContext(attackInput));
            return
                new CombatResult(0,
                    Result.Success); //returns success because even if the attack missed, it was still a valid action
        }

        PublishAttackEvent(attackInput, false);
        aggressor?.PreAttack(CreateCombatContext(attackInput));

        var damage = DamageCalculation.Calculate(attackInput);
        var damageResult = PerformAttack(aggressor, target, damage);

        if (attackInput.Parameters.FieldAttack) CreateMagicField(attackInput);

        if (damageResult.WasDamaged)
        {
            conditionAttackService.Execute(attackInput);
            return new CombatResult(damageResult.DamageList.TotalDamage, Result.Success);
        }

        if (damage.MainDamage is null or { Damage: <= 0 }) conditionAttackService.Execute(attackInput);

        return new CombatResult(damageResult.DamageList.TotalDamage, Result.Success);
    }

    private static bool IsAttackMissed(AttackInput attackInput)
    {
        if (!attackInput.Parameters.HitChance.HasValue)
            return false;

        var value = GameRandom.Random.Next(1, maxValue: 100);
        return value > attackInput.Parameters.HitChance;
    }

    private void PublishAttackEvent(
        AttackInput attackInput,
        bool attackMissed)
    {
        eventAggregator.Publish(new CreatureAttackingEvent(
            attackInput.Aggressor,
            attackInput.Target,
            attackInput.Parameters.ShootType,
            attackInput.Parameters.Effect,
            attackMissed));
    }

    private CombatContext CreateCombatContext(AttackInput attackInput)
    {
        return new CombatContext
        {
            CombatParameters = attackInput.Parameters,
            InfiniteAmmo = combatConfiguration.InfiniteAmmo,
            InfiniteThrowingWeapon = combatConfiguration.InfiniteThrowingWeapon
        };
    }

    private static DamageResult PerformAttack(ICombatActor aggressor, IThing target, CalculatedAttackDamage damage)
    {
        if (target is not ICombatActor targetCreature || damage.MainDamage == null || Equals(target, aggressor))
            return new DamageResult(new CombatDamageList(), false);

        var unjustifiedAttack =
            target is IPlayer targetPlayer && aggressor is IPlayer playerAggressor &&
            playerAggressor.GetSkull(targetPlayer) is Skull.None;

        var mainDamage = damage.MainDamage;

        if (mainDamage is null || mainDamage.Type is DamageType.None)
            return new DamageResult(new CombatDamageList(), false);

        mainDamage.Unjustified = unjustifiedAttack;

        if (damage.ExtraDamage?.Damage > 0)
        {
            var damages = new CombatDamageList([mainDamage, damage.ExtraDamage]);
            return targetCreature.TakeDamage(aggressor, damages);
        }

        return targetCreature.TakeDamage(aggressor, mainDamage);
    }

    private void CreateMagicField(AttackInput attackInput)
    {
        var magicFieldType = attackInput.Parameters.DamageType switch
        {
            DamageType.Earth => MagicFieldType.Poison,
            DamageType.Energy => MagicFieldType.Energy,
            DamageType.Fire => MagicFieldType.Fire,
            _ => MagicFieldType.None
        };

        magicFieldService.AddToGround(
            attackInput.Aggressor as ICreature,
            attackInput.Target.Location,
            magicFieldType);
    }
}