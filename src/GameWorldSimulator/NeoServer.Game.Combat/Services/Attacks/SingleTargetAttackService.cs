using NeoServer.Game.Combat.Services.Attacks.Builders;
using NeoServer.Game.Combat.Services.Attacks.Events;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Enums;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Results;

namespace NeoServer.Game.Combat.Services.Attacks;

public class SingleTargetAttackService(
    IEventAggregator eventAggregator,
    CombatConfiguration combatConfiguration,
    CombatBloodPoolService combatBloodPoolService,
    IMap map)
    : IAttackService
{
    public Result Execute(AttackInput attackInput)
    {
        var aggressor = attackInput.Aggressor as ICombatActor;
        var target = attackInput.Target ?? aggressor?.CurrentTarget;

        var attackMissed = false;

        if (attackInput.Parameters.HitChance.HasValue)
        {
            var value = GameRandom.Random.Next(1, maxValue: 100);
            attackMissed = value > attackInput.Parameters.HitChance;
        }
        
        eventAggregator.Publish(new CreatureAttackingEvent(aggressor, target, attackInput.Parameters.ShootType,
            attackInput.Parameters.Effect, attackMissed));

        aggressor?.PreAttack(new CombatContext()
        {
            CombatParameters = attackInput.Parameters,
            InfiniteAmmo = combatConfiguration.InfiniteAmmo,
            InfiniteThrowingWeapon = combatConfiguration.InfiniteThrowingWeapon
        });

        if (attackMissed) return Result.Success;

        var damage = DamageBuilder.Build(attackInput);

        PerformAttack(aggressor, target, damage);

        CreateBloodPool(damage, target);

        return Result.Success;
    }

    private void CreateBloodPool(CalculatedAttackDamage damage, IThing target)
    {
        if (damage.MainDamage is { Damage: > 0, IsElementalDamage: false })
        {
            combatBloodPoolService.CreateSplash(target as ICombatActor, damage.MainDamage);
            return;
        }

        if (damage.ExtraDamage is { Damage: > 0, IsElementalDamage: false })
        {
            combatBloodPoolService.CreateSplash(target as ICombatActor, damage.ExtraDamage);
        }
    }

    private static void PerformAttack(ICombatActor aggressor, IThing target, CalculatedAttackDamage damage)
    {
        if (target is not ICombatActor combatActor) return;
        
        //cannot attack himself
        if(Equals(target, aggressor)) return;

        var unjustifiedAttack =
            target is IPlayer targetPlayer && aggressor is IPlayer playerAggressor &&
            playerAggressor.GetSkull(targetPlayer) is Skull.None;

        var mainDamage = damage.MainDamage;
        mainDamage.Unjustified = unjustifiedAttack;

        if (damage.ExtraDamage.Damage > 0)
        {
            var damages = new CombatDamageList([mainDamage, damage.ExtraDamage]);
            combatActor.TakeDamage(aggressor, damages);
            return;
        }

        combatActor.TakeDamage(aggressor, damage.MainDamage);
    }

   
}