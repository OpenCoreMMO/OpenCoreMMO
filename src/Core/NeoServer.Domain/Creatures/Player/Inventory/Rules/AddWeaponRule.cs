using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Items.Types.Body;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.Creatures.Player.Inventory.Rules;

public static class AddWeaponRule
{
    public static Result CanAddWeapon(this Inventory inventory, Slot slot, IWeapon weapon)
    {
        if (slot != Slot.Left) return Result.Fail(InvalidOperation.CannotDress);

        var hasShieldDressed = inventory[Slot.Right] != null;

        if (weapon.TwoHanded && hasShieldDressed)
            //trying to add a two handed while right slot has a shield
            return Result.Fail(InvalidOperation.BothHandsNeedToBeFree);

        return Result.Success;
    }
}