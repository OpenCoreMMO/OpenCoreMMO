using System;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types;
using NeoServer.Game.Common.Contracts.Items.Types.Body;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Results;

namespace NeoServer.Game.Creatures.Player.Inventory.Rules;

internal static class AddToSlotRule
{
    public static Result CanAddItem(this Inventory inventory, Slot slot, IItem item, byte amount)
    {
        if (Guard.AnyNull(slot, item)) return Result.NotPossible;

        if (!item.IsPickupable) return Result.NotPossible;

        if (!CanCarryItem(inventory, item, slot, amount)) return Result.Fail(InvalidOperation.TooHeavy);

        return CanAddItemToSlot(inventory, slot, item);
    }

    private static Result CanAddItemToSlot(this Inventory inventory, Slot slot, IItem item)
    {
        var cannotDressFail = Result.Fail(InvalidOperation.CannotDress);

        if (slot == Slot.Backpack) return inventory.CanAddToBackpackSlot(item);

        if (item is not IInventoryEquipment inventoryItem) return cannotDressFail;

        if (item is IEquipmentRequirement requirement && !requirement.CanBeDressed(inventory.Owner))
            return cannotDressFail;

        if (inventoryItem is IWeapon weapon) return inventory.CanAddWeapon(slot, weapon);

        if (slot == Slot.Right && inventory[Slot.Left] is IWeapon { TwoHanded: true })
            //trying to add a shield while left slot has a two handed weapon
            return Result.Fail(InvalidOperation.BothHandsNeedToBeFree);

        return inventoryItem.Slot != slot ? cannotDressFail : Result.Success;
    }

    private static bool CanCarryItem(Inventory inventory, IItem item, Slot slot, byte amount = 1)
    {
        var itemWeight = item is ICumulative c ? c.CalculateWeight(amount) : item.Weight;

        if (SwapRule.ShouldSwap(inventory, item, slot))
        {
            var itemOnSlot = inventory.InventoryMap.GetItem<IItem>(slot);

            return inventory.TotalWeight - itemOnSlot.Weight + itemWeight <= inventory.Owner.TotalCapacity;
        }

        var weight = item.Weight;

        if (item is ICumulative cumulative && slot == Slot.Ammo)
        {
            var amountToAdd = cumulative.Amount > cumulative.AmountToComplete
                ? cumulative.AmountToComplete
                : cumulative.Amount;
            weight = cumulative.CalculateWeight(amountToAdd);
        }

        var canCarry = inventory.TotalWeight + weight <= inventory.Owner.TotalCapacity;
        return canCarry;
    }

    public static Result<uint> CanAddItem(Inventory inventory, IItemType itemType)
    {
        if (itemType is null) return Result<uint>.NotPossible;
        if (itemType.BodyPosition == Slot.None) return new Result<uint>(InvalidOperation.NotEnoughRoom);

        var itemOnSlot = inventory[itemType.BodyPosition];
        if (itemOnSlot is not null && itemType.ServerId != itemOnSlot.Metadata.ServerId)
            return new Result<uint>(InvalidOperation.NotEnoughRoom);

        byte possibleAmountToAdd;
        if (ICumulative.IsApplicable(itemType))
        {
            var amountOnSlot = inventory[itemType.BodyPosition]?.Amount ?? 0;
            possibleAmountToAdd = (byte)Math.Abs(100 - amountOnSlot);
        }
        else
        {
            if (itemOnSlot is not null) return new Result<uint>(InvalidOperation.NotEnoughRoom);
            possibleAmountToAdd = 1;
        }

        return possibleAmountToAdd == 0
            ? new Result<uint>(InvalidOperation.NotEnoughRoom)
            : new Result<uint>(possibleAmountToAdd);
    }
}