using NeoServer.Domain.Combat.Services.Attacks.Builders;
using NeoServer.Domain.Combat.Services.Attacks.Events;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Enums;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Services;

namespace NeoServer.Domain.Combat.Services.Attacks;

public class SingleTargetAttackService(
    IEventAggregator eventAggregator,
    CombatConfiguration combatConfiguration,
    ConditionAttackService conditionAttackService,
    MagicFieldService magicFieldService)
    : IAttackService
{
    public Result Execute(AttackInput attackInput)
    {
        var aggressor = attackInput.Aggressor as ICombatActor;
        var target = attackInput.Target ?? aggressor?.CurrentTarget;

        if (IsAttackMissed(attackInput))
        {
            PublishAttackEvent(attackInput, true);
            aggressor?.PreAttack(CreateCombatContext(attackInput));
            return Result.Success;
        }

        PublishAttackEvent(attackInput, false);
        aggressor?.PreAttack(CreateCombatContext(attackInput));

        var damage = DamageBuilder.Build(attackInput);
        var wasDamaged = PerformAttack(aggressor, target, damage);

        if (attackInput.Parameters.FieldAttack) CreateMagicField(attackInput);

        if (wasDamaged)
        {
            conditionAttackService.Execute(attackInput);
            return Result.Success;
        }

        if (damage.MainDamage is null or { Damage: <= 0 }) conditionAttackService.Execute(attackInput);

        return Result.Success;
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

    private static bool PerformAttack(ICombatActor aggressor, IThing target, CalculatedAttackDamage damage)
    {
        if (target is not ICombatActor targetCreature)
            return false;

        if (damage.MainDamage == null)
            return false;

        if (Equals(target, aggressor))
            return false;

        var unjustifiedAttack =
            target is IPlayer targetPlayer && aggressor is IPlayer playerAggressor &&
            playerAggressor.GetSkull(targetPlayer) is Skull.None;

        var mainDamage = damage.MainDamage;

        if (mainDamage is null || mainDamage.Type is DamageType.None) return false;

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