using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types.Body;
using NeoServer.Game.Common.Contracts.Items.Weapons;
using NeoServer.Game.Common.Contracts.Items.Weapons.Attributes;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Parsers;

namespace NeoServer.Game.Combat.Services.Attacks.Builders.AttackParameter;

public static class PlayerAttackParameterBuilder
{
    public static Common.Combat.Structs.AttackParameter Build(IPlayer player, IThing target)
    {
        if (player is null) return default;

        var elementalDamage = CalculateElementalAttack(player);

        return new Common.Combat.Structs.AttackParameter
        {
            Type = AttackType.Regular,
            MinDamage = player.MinimumAttackPower,
            MaxDamage = player.MaximumAttackPower,
            DamageType = GetDamageType(player),
            Range = (byte)(player.Inventory.Weapon is IHasRange weapon ? weapon.Range : 0),
            Effect = EffectT.None,
            Spread = 5,
            ShootType = GetShootType(player),
            ExtraAttack = elementalDamage,
            CooldownType = CooldownType.Combat,
            CooldownDuration = (int)player.AttackSpeed,
            IsMagicalAttack = player.Inventory.Weapon is IMagicalWeapon,
            HitChance = HitChanceCalculation.GetHitChance(player.Inventory.Weapon,
                player?.GetSkillLevel(player.SkillInUse) ?? 0,
                (byte)player.Location.GetSqmDistance(target.Location)),
        };
    }

    // public static AttackParameter Build(IPlayer player, IAttackRune rune, IThing target)
    // {
    //     var area = new AreaAttackParameter();
    //     if (!string.IsNullOrWhiteSpace(rune.Area))
    //     {
    //         var areaEffectStore = IoC.GetInstance<IAreaEffectStore>();
    //         var areaTemplate = areaEffectStore.Get(rune.Area);
    //
    //         area.SetArea(AreaEffect.Create(target.Location, areaTemplate), rune.Effect);
    //     }
    //
    //     if (player is null) return default;
    //
    //     var minMaxDamage = RuneAttackCalculation.Calculate(player, rune);
    //
    //     return new AttackParameter
    //     {
    //         MinDamage = (ushort)minMaxDamage.Min,
    //         MaxDamage = (ushort)minMaxDamage.Max,
    //         DamageType = rune.DamageType,
    //         ShootType = rune.Metadata.ShootType,
    //         ExtraAttack = default,
    //         Cooldown = rune.CooldownTime,
    //         CooldownType = CooldownType.Rune,
    //         IsMagicalAttack = true,
    //         NeedTarget = rune.NeedTarget,
    //         Area = area,
    //         Effect = rune.Effect
    //     };
    // }
    //
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
            IThrowableWeapon throwableDistanceWeapon => throwableDistanceWeapon.Metadata.ShootType,
            IMagicalWeapon magicWeapon => magicWeapon.Metadata.ShootType,
            _ => ShootType.None
        };
    }

    //
    // private static string GetAttackName(IPlayer player)
    // {
    //     return player.Inventory.IsUsingDistanceWeapon
    //         ? nameof(DistanceAttackStrategy)
    //         : nameof(MeleeAttackStrategy);
    // }
    //
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