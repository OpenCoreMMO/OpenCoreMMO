using NeoServer.Game.Common.Contracts.Items.Types.Body;

namespace NeoServer.Game.Combat.Services.Attacks;

public static class HitChanceCalculation
{
    public static byte GetHitChance(IWeapon weapon, ushort skill, byte distance)
    {
        byte hitChance = 100;

        if (weapon is IDistanceWeapon distanceWeapon)
            hitChance =
                (byte)(DistanceHitChanceCalculation.CalculateFor2Hands(skill, distance) +
                       distanceWeapon.ExtraHitChance);

        if (weapon is IThrowableWeapon throwableDistanceWeapon)
            hitChance =
                (byte)(DistanceHitChanceCalculation.CalculateFor1Hand(skill, distance) +
                       throwableDistanceWeapon.ExtraHitChance);

        return hitChance;
    }
}