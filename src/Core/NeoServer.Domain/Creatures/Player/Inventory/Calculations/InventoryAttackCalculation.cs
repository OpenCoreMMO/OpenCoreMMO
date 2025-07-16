using NeoServer.Domain.Common.Combat;
using NeoServer.Domain.Common.Contracts.Items.Types.Body;
using NeoServer.Domain.Common.Contracts.Items.Weapons.Attributes;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Items.Items.Weapons;

namespace NeoServer.Domain.Creatures.Player.Inventory.Calculations;

internal static class InventoryAttackCalculation
{
    internal static ElementalDamage CalculateTotalElementalAttack(this Inventory inventory)
    {
        ushort attack = 0;

        var weapon = inventory.Weapon;

        var damageType = DamageType.None;

        if (weapon is IHasAttack hasAttack)
        {
            attack += hasAttack.WeaponAttack.ElementalDamage.AttackPower;
            damageType = hasAttack.WeaponAttack.ElementalDamage.DamageType;
        }

        if (weapon is INeedsAmmo needsAmmo && needsAmmo.CanShootAmmunition(inventory.Ammo))
        {
            attack += inventory.Ammo.WeaponAttack.ElementalDamage.AttackPower;
            damageType = inventory.Ammo.WeaponAttack.ElementalDamage.DamageType;
        }

        return new ElementalDamage(damageType, (byte)attack);
    }

    internal static ushort CalculateTotalAttack(this Inventory inventory)
    {
        ushort attack = 0;

        var weapon = inventory.Weapon;

        if (weapon is IHasAttack hasAttack) attack += hasAttack.WeaponAttack.AttackPower;

        if (weapon is IHasAttackBonus) attack += weapon.AttackPower;

        if (weapon is INeedsAmmo needsAmmo && needsAmmo.CanShootAmmunition(inventory.Ammo))
            attack += inventory.Ammo.WeaponAttack.AttackPower;

        return attack;
    }

    internal static byte CalculateAttackRange(this InventoryMap inventoryMap)
    {
        if (inventoryMap.GetItem<IDistanceWeapon>(Slot.Left) is { } leftWeapon)
            return leftWeapon.Range;

        if (inventoryMap.GetItem<ThrowableWeapon>(Slot.Left) is { } rightWeapon)
            return rightWeapon.Range;

        return 0;
    }
}