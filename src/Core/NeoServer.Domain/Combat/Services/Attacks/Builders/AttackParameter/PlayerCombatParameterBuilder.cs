using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types.Body;
using NeoServer.Domain.Common.Contracts.Items.Weapons;
using NeoServer.Domain.Common.Contracts.Items.Weapons.Attributes;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Parsers;
using NeoServer.Domain.Items.Items.Weapons;

namespace NeoServer.Domain.Combat.Services.Attacks.Builders.AttackParameter;

public static class PlayerCombatParameterBuilder
{
    public static CombatParameter Build(IPlayer player, IThing target)
    {
        if (player is null) return default;

        var elementalDamage = CalculateElementalAttack(player);

        return new CombatParameter
        {
            MinDamage = player.MinimumAttackPower,
            MaxDamage = player.MaximumAttackPower,
            DamageType = GetDamageType(player),
            Range = player.Inventory.Weapon is IHasRange weapon ? weapon.Range : null,
            Effect = EffectT.None,
            Spread = 5,
            ShootType = GetShootType(player),
            ExtraAttack = elementalDamage,
            CooldownType = CooldownType.Combat,
            CooldownDuration = (uint)player.AttackSpeed,
            IsMagicalAttack = player.Inventory.Weapon is IMagicalWeapon,
            UsingWeapon = true,
            HitChance = HitChanceCalculation.GetHitChance(player.Inventory.Weapon,
                player?.GetSkillLevel(player.SkillInUse) ?? 0,
                (byte)player.Location.GetSqmDistance(target.Location))
        };
    }


    private static DamageType GetDamageType(IPlayer player)
    {
        if (player.Inventory.Weapon is null) return DamageType.Physical;

        if (player.Inventory.Weapon is IMagicalWeapon) return player.Inventory.Weapon.Metadata.ShootType.ToDamageType();

        if (player.Inventory.Weapon is IDistanceWeapon)
            return player.Inventory.Ammo?.Metadata?.DamageType ?? DamageType.Physical;

        return player.Inventory.Weapon.Metadata.DamageType;
    }

    //
    private static ShootType GetShootType(IPlayer player)
    {
        var weapon = player.Inventory.Weapon;
        var ammo = player.Inventory.Ammo;

        return weapon switch
        {
            INeedsAmmo distanceWeapon when distanceWeapon.CanShootAmmunition(player.Inventory.Ammo) =>
                ammo?.ShootType ?? ShootType.None,
            ThrowableWeapon throwableDistanceWeapon => throwableDistanceWeapon.Metadata.ShootType,
            IMagicalWeapon magicWeapon => magicWeapon.Metadata.ShootType,
            _ => ShootType.None
        };
    }


    private static ExtraAttack CalculateElementalAttack(ICombatActor aggressor)
    {
        if (aggressor.MaximumElementalAttackPower is 0) return default;

        if (aggressor is not IPlayer player) return default;

        var damageType = player.Inventory.TotalElementalAttack.DamageType;

        if (damageType == DamageType.None) return default;

        return new ExtraAttack
        {
            MinDamage = aggressor.MinimumAttackPower,
            MaxDamage = aggressor.MaximumElementalAttackPower,
            DamageType = damageType
        };
    }
}