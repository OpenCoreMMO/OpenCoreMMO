using NeoServer.Domain.Common.Item;

namespace NeoServer.Domain.Common.Parsers;

public static class ShootTypeParser
{
    public static ShootType Parse(string shootType)
    {
        return shootType?.ToLower() switch
        {
            "arrow" => ShootType.Arrow,
            "bolt" => ShootType.Bolt,
            "burstarrow" => ShootType.BurstArrow,
            "death" => ShootType.Death,
            "earth" => ShootType.Earth,
            "eartharrow" => ShootType.EarthArrow,
            "enchantedspear" => ShootType.EnchantedSpear,
            "energy" => ShootType.Energy,
            "explosion" => ShootType.Explosion,
            "fire" => ShootType.Fire,
            "flammingarrow" => ShootType.FlammingArrow,
            "flasharrow" => ShootType.FlashArrow,
            "greenstar" => ShootType.GreenStar,
            "holy" => ShootType.Holy,
            "huntingspear" => ShootType.HuntingSpear,
            "ice" => ShootType.Ice,
            "infernalbolt" => ShootType.InfernalBolt,
            "largerock" => ShootType.LargeRock,
            "onyxarrow" => ShootType.OnyxArrow,
            "piercingbolt" => ShootType.PiercingBolt,
            "poison" => ShootType.Poison,
            "poisonarrow" => ShootType.PoisonArrow,
            "powerbolt" => ShootType.PowerBolt,
            "redstar" => ShootType.RedStar,
            "royalspear" => ShootType.RoyalSpear,
            "shiverarrow" => ShootType.ShiverArrow,
            "smallearth" => ShootType.SmallEarth,
            "smallholy" => ShootType.SmallHoly,
            "smallice" => ShootType.SmallIce,
            "smallstone" => ShootType.SmallStone,
            "snowball" => ShootType.SnowBall,
            "spear" => ShootType.Spear,
            "suddendeath" => ShootType.SuddenDeath,
            "sniperarrow" => ShootType.SniperArrow,
            "throwingknife" => ShootType.ThrowingKnife,
            "throwingstar" => ShootType.ThrowingStar,
            _ => ShootType.None
        };
    }

    public static DamageType ToDamageType(this ShootType shootType)
    {
        return shootType switch
        {
            ShootType.Energy => DamageType.Energy,
            ShootType.EnergyBall => DamageType.Energy,
            ShootType.Fire => DamageType.Fire,
            ShootType.Earth => DamageType.Earth,
            ShootType.Death => DamageType.Death,
            ShootType.Holy => DamageType.Holy,
            ShootType.Ice => DamageType.Ice,
            ShootType.Poison => DamageType.Earth,
            ShootType.SmallEarth => DamageType.Earth,
            ShootType.SmallHoly => DamageType.Holy,
            ShootType.SmallIce => DamageType.Ice,
            _ => DamageType.None
        };
    }
}