using System;
using System.Collections.Immutable;
using System.Diagnostics;
using NeoServer.Game.Combat.Calculations;
using NeoServer.Game.Combat.Services.Attacks.Events;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Enums;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Effects.Magical;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Results;

namespace NeoServer.Game.Combat.Services.Attacks;

public class RegularAttackService(IEventAggregator eventAggregator, CombatConfiguration combatConfiguration)
    : IAttackService
{
    public Result Execute(AttackInput attackInput)
    {
        var aggressor = attackInput.Aggressor as ICombatActor;
        var target = attackInput.Target;

        if (Math.Max(attackInput.Parameters.Range, (byte)1) < target.Location.GetMaxSqmDistance(aggressor.Location))
        {
            //the aggressor is not close enough to the target
            return Result.Fail(InvalidOperation.TooFar);
        }

        if (attackInput.Parameters.NeedTarget && target is not ICombatActor)
        {
            return Result.NotPossible;
        }

        var damage = BuildDamages(attackInput);

        var attackMissed = false;

        if (attackInput.Parameters.HitChance < 100)
        {
            var value = GameRandom.Random.Next(1, maxValue: 100);
            attackMissed = value > attackInput.Parameters.HitChance;
        }

        if (attackInput.Parameters.Spread > 0)
        {
            var coordinates = SpreadEffect.Create(aggressor.Direction, attackInput.Parameters.Length,
                attackInput.Parameters.Spread);

            attackInput.Parameters.Area.SetArea(coordinates, attackInput.Parameters.Effect, true);
        }

        eventAggregator.Publish(new CreatureAttackedEvent(attackInput, attackMissed));

        aggressor?.PreAttack(new CombatContext()
        {
            AttackParameters = attackInput.Parameters,
            InfiniteAmmo = combatConfiguration.InfiniteAmmo,
            InfiniteThrowingWeapon = combatConfiguration.InfiniteThrowingWeapon
        });

        if (attackMissed) return Result.Success;

        PerformAttack(target, damage, aggressor);

        return Result.Success;
    }

    private static void PerformAttack(IThing target, CalculatedAttackDamage damage, ICombatActor aggressor)
    {
        if (target is not ICombatActor combatActor) return;

        var unjustifiedAttack =
            target is IPlayer targetPlayer && aggressor is IPlayer playerAggressor &&
            playerAggressor.GetSkull(targetPlayer) is Skull.None;

        var mainDamage = damage.MainDamage;
        mainDamage.Unjustified = unjustifiedAttack;

        if (damage.ExtraDamage.Damage > 0)
        {
            var damages = new CombatDamageList([mainDamage, damage.ExtraDamage]);
            combatActor.ReceiveAttack(aggressor, damages);
            return;
        }

        combatActor.ReceiveAttack(aggressor, damage.MainDamage);
    }

    /// <summary>
    /// Calculates damage to the target
    /// </summary>
    private CalculatedAttackDamage BuildDamages(in AttackInput attackInput)
    {
        // Allocate space for one or two damages based on whether there is an extra attack
        var damage = new CalculatedAttackDamage();

        var extraAttack = attackInput.Parameters.ExtraAttack;

        var physicalDamage = AttackCalculation.Calculate(attackInput.Parameters.MinDamage,
            attackInput.Parameters.MaxDamage,
            attackInput.Parameters.DamageType);

        damage.MainDamage = physicalDamage;

        // If there's an extra elemental attack, calculate and add it to the buffer
        if (attackInput.Parameters.HasExtraAttack)
        {
            //Adds an elemental attack to the damage buffer, using the extra attack parameters.
            damage.ExtraDamage = AttackCalculation.Calculate(extraAttack.MinDamage,
                extraAttack.MaxDamage, extraAttack.DamageType);

            Debug.WriteLine(damage.ExtraDamage.Damage);
        }

        //var combatDamageList = new CombatDamageList(damages);

        // Handle area attacks separately by propagating damage to all affected targets

        // if (attackInput.Parameters.IsAttackInArea)
        // {
        //     areaAttackProcessor.Propagate(attackInput, combatDamageList);
        //     return;
        // }
        //
        // // Handle defense logic based on the type of target (player or monster)
        // defenseHandler.Handle(attackInput.Aggressor,  attackInput.Target as ICombatActor, combatDamageList);
        return damage;
    }
}