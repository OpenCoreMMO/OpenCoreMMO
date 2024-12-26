using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Creatures.Players;

namespace NeoServer.Game.Systems.SafeTrade.Operations;

internal static class TradeSlotDestinationQuery
{
    public static Slot Get(IPlayer player, IItem itemToAdd, IItem itemToBeRemoved)
    {
        if (itemToAdd.Metadata.BodyPosition is Slot.None) return Slot.Backpack;
        var existingItemOnSlot = player.Inventory[itemToAdd.Metadata.BodyPosition];

        if (itemToAdd.IsCumulative &&
            existingItemOnSlot?.ServerId == itemToAdd.ServerId &&
            itemToAdd.Amount + existingItemOnSlot.Amount == 100) return itemToAdd.Metadata.BodyPosition;

        var slotDestination = GetTwoHandedWeaponSlotDestination(player, itemToAdd, itemToBeRemoved);
        if (slotDestination is not Slot.None) return slotDestination;

        slotDestination = existingItemOnSlot is null || existingItemOnSlot == itemToBeRemoved
            ? itemToAdd.Metadata.BodyPosition
            : Slot.Backpack;

        if (slotDestination == Slot.Backpack) return slotDestination;

        return player.Inventory.CanAddItem(itemToAdd, itemToAdd.Amount, (byte)slotDestination).Succeeded
            ? slotDestination
            : Slot.Backpack;
    }

    private static Slot GetTwoHandedWeaponSlotDestination(IPlayer player, IItem itemToAdd, IItem itemToBeRemoved)
    {
        if (itemToAdd.Metadata.BodyPosition is not Slot.TwoHanded) return Slot.None;

        var isRemovingTheShield = itemToBeRemoved == player.Inventory[Slot.Right];
        var isRemovingTheWeapon = itemToBeRemoved == player.Inventory.Weapon;

        var hasNoShield = isRemovingTheShield || !player.Inventory.HasShield;
        var hasNoWeapon = isRemovingTheWeapon || !player.Inventory.IsUsingWeapon;

        if (hasNoWeapon && hasNoShield) return Slot.Left;

        return Slot.Backpack;
    }
}