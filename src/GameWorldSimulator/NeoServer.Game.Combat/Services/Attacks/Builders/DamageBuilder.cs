using NeoServer.Game.Common.Combat.Enums;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;

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

        ushort factor = 1;

        if (attackInput.Aggressor is IPlayer && attackInput.Target is IPlayer targetPlayer &&
            targetPlayer.Skull != Skull.Black)
        {
            factor = 2;
        }

        var physicalDamage = AttackCalculation.Calculate(
            (ushort)(attackInput.Parameters.MinDamage / factor),
            (ushort)(attackInput.Parameters.MaxDamage / factor),
            attackInput.Parameters.DamageType);

        // If there's an extra elemental attack, calculate and add it to the buffer
        if (attackInput.Parameters.HasExtraAttack)
        {
            //Adds an elemental attack to the damage buffer, using the extra attack parameters.
            damage.ExtraDamage = AttackCalculation.Calculate(
                (ushort)(extraAttack.MinDamage / factor),
                (ushort)(extraAttack.MaxDamage / factor),
                extraAttack.DamageType);
        }

        damage.MainDamage = physicalDamage;


        return damage;
    }
}