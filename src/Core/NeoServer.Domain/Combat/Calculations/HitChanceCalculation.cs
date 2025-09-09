using NeoServer.Domain.Common.Contracts.Items.Types.Body;
using NeoServer.Domain.Items.Items.Weapons;

namespace NeoServer.Domain.Combat.Calculations;

public static class HitChanceCalculation
{
    public static byte GetHitChance(IWeapon weapon, ushort skill, byte distance)
    {
        byte hitChance = 100;

        if (weapon is MagicWeapon)
        {
            return 100;
        }

        if (weapon is IDistanceWeapon distanceWeapon)
        {
            hitChance =
                (byte)(DistanceHitChanceCalculation.CalculateFor2Hands(skill, distance) +
                       distanceWeapon.ExtraHitChance);
        }

        if (weapon is ThrowableWeapon throwableDistanceWeapon)
        {
            hitChance =
                (byte)(DistanceHitChanceCalculation.CalculateFor1Hand(skill, distance) +
                       throwableDistanceWeapon.ExtraHitChance);
        }

        return hitChance;
    }
}