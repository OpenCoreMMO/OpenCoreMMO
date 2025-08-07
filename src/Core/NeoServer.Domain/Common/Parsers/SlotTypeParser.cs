using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Items;

namespace NeoServer.Domain.Common.Parsers;

public class SlotTypeParser
{
    public static Slot Parse(ItemTypeAttributeList itemAttributes)
    {
        if (itemAttributes is null) return Slot.None;

        var slotType = itemAttributes.GetAttribute(ItemTypeAttribute.BodyPosition);

        if (slotType is null && itemAttributes.TryGetAttribute(ItemTypeAttribute.WeaponType, out var weaponType))
            slotType = weaponType;

        return slotType switch
        {
            "body" => Slot.Body,
            "legs" => Slot.Legs,
            "head" => Slot.Head,
            "feet" => Slot.Feet,
            "shield" => Slot.Right,
            "ammo" => Slot.Ammo,
            "backpack" => Slot.Backpack,
            "ring" => Slot.Ring,
            "necklace" => Slot.Necklace,
            "two-handed" => Slot.TwoHanded,
            "weapon" => Slot.Left,
            "club" => Slot.Left,
            "distance" => Slot.Left,
            "sword" => Slot.Left,
            "axe" => Slot.Left,
            "ammunition" => Slot.Ammo,
            _ => Slot.None
        };
    }
}