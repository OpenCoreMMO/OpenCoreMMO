using System;
using System.Diagnostics;
using NeoServer.Game.Combat.Services.Attacks.Events;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Effects.Magical;
using NeoServer.Game.Common.Results;

namespace NeoServer.Game.Combat.Services.Attacks;

public class RegularAttackService(IEventAggregator eventAggregator) : IAttackService
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
        
        var damage = BuildDamages(attackInput);
        
        if (attackInput.Parameters.Spread > 0)
        {
            var coordinates = SpreadEffect.Create(aggressor.Direction, attackInput.Parameters.Length,
                attackInput.Parameters.Spread);
            
            attackInput.Parameters.Area.SetArea(coordinates, attackInput.Parameters.Effect, true);
        }

        eventAggregator.Publish(new CreatureAttackedEvent(attackInput));

        aggressor?.PreAttack(attackInput.Parameters);

        if (target is ICombatActor combatActor)
        {
            combatActor.ReceiveAttack(aggressor, damage.MainDamage);
            combatActor.ReceiveAttack(aggressor, damage.ExtraDamage);
        }
        
        return Result.Success;
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