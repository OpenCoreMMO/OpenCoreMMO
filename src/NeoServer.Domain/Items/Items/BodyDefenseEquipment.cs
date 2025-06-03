using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types.Body;
using NeoServer.Domain.Common.Creatures.Players;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Bases;

namespace NeoServer.Domain.Items.Items;

public class BodyDefenseEquipment : Equipment, IDefenseEquipment
{
    public BodyDefenseEquipment(IItemType itemType, Location location)
        : base(itemType, location)
    {
    }

    protected override string PartialInspectionText
    {
        get
        {
            var hasArmorValue = Metadata.Attributes.TryGetAttribute<byte>(ItemAttribute.Armor, out var armorValue);
            if (hasArmorValue) return $"Arm: {armorValue}";

            var hasDefenseValue =
                Metadata.Attributes.TryGetAttribute<byte>(ItemAttribute.Defense, out var defenseValue);
            return hasDefenseValue ? $"Def: {defenseValue}" : string.Empty;
        }
    }

    public override bool CanBeDressed(IPlayer player)
    {
        var hasRequiredVocation = Guard.IsNullOrEmpty(Vocations);
        var hasMinimumLevel = MinLevel == 0;

        if (Vocations is not null)
            foreach (var vocation in Vocations)
                if (vocation == player.VocationType && player.Level >= MinLevel)
                    hasRequiredVocation = true;

        if (player.Level >= MinLevel) hasMinimumLevel = true;
        return hasRequiredVocation && hasMinimumLevel;
    }

    public bool Pickupable => true;

    public Slot Slot => Metadata.WeaponType == WeaponType.Shield ? Slot.Right : Metadata.BodyPosition;

    public virtual void OnMoved(IThing to)
    {
    }

    public static bool IsApplicable(IItemType type)
    {
        return type?.Group is ItemGroup.BodyDefenseEquipment;
    }
}