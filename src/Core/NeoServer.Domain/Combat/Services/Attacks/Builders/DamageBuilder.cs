using NeoServer.Domain.Common.Combat.Enums;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Item;

namespace NeoServer.Domain.Combat.Services.Attacks.Builders;

public static class DamageBuilder
{
    /// <summary>
    ///     Calculates damage to the target
    /// </summary>
    public static CalculatedAttackDamage Build(in AttackInput attackInput)
    {
        // Allocate space for one or two damages based on whether there is an extra attack
        var damage = new CalculatedAttackDamage();

        var extraAttack = attackInput.Parameters.ExtraAttack;

        ushort factor = 1;

        if (attackInput.Aggressor is IPlayer && attackInput.Target is IPlayer targetPlayer &&
            targetPlayer.Skull != Skull.Black)
            factor = 2;

        if (attackInput.Parameters.DamageType is DamageType.None) return damage;

        var mainDamage = AttackCalculation.Calculate(
            (ushort)(attackInput.Parameters.MinDamage / factor),
            (ushort)(attackInput.Parameters.MaxDamage / factor),
            attackInput.Parameters.DamageType);

        mainDamage.Effect = attackInput.Parameters.Effect;
        damage.MainDamage = mainDamage;

        // If there's an extra elemental attack, calculate and add it to the buffer
        if (attackInput.Parameters.HasExtraAttack)
            //Adds an elemental attack to the damage buffer, using the extra attack parameters.
            damage.ExtraDamage = AttackCalculation.Calculate(
                (ushort)(extraAttack.MinDamage / factor),
                (ushort)(extraAttack.MaxDamage / factor),
                extraAttack.DamageType);

        return damage;
    }
}