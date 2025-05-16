using System.Diagnostics;
using NeoServer.Game.Common.Combat.Structs;

namespace NeoServer.Game.Combat.Services.Attacks.Builders;

public class DamageBuilder
{
    /// <summary>
    /// Calculates damage to the target
    /// </summary>
    public static CalculatedAttackDamage Build(in AttackInput attackInput)
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