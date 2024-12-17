using NeoServer.Game.Common.Contracts.Items.Types.Body;
using NeoServer.Game.Common.Creatures.Players;

namespace NeoServer.Game.Creatures.Player.Inventory.Calculations;

internal static class InventoryAttackCalculation
{
    internal static ushort CalculateTotalAttack(this Inventory inventory)
    {
        ushort attack = 0;

        switch (inventory.Weapon)
        {
            case IWeaponItem weapon:
                return weapon.AttackPower;
            case IDistanceWeapon distance:
            {
                attack += distance.ExtraAttack;
                if (inventory.Ammo != null) attack += inventory.Ammo.Attack;
                break;
            }
            case IThrowableDistanceWeaponItem distance:
            {
                return distance.AttackPower;
            }
        }

        return attack;
    }

    internal static byte CalculateAttackRange(this InventoryMap inventoryMap)
    {
        if (inventoryMap.GetItem<IDistanceWeapon>(Slot.Left) is { } leftWeapon)
            return leftWeapon.Range;

        if (inventoryMap.GetItem<IThrowableDistanceWeaponItem>(Slot.Left) is { } rightWeapon)
            return rightWeapon.Range;

        return 0;
    }
}