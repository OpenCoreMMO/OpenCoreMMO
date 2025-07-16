using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types.Body;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Player.Inventory;
using NeoServer.Domain.Items.Bases;

namespace NeoServer.Domain.Items.Items;

public class BodyDefenseEquipment : Equipment, IBodyEquipmentEquipment
{
    public new ushort Defense => base.Defense > 0
        ? base.Defense
        : base.Armor;

    public bool Pickupable => true;

    public Slot Slot => Metadata.WeaponType == WeaponType.Shield ? Slot.Right : Metadata.BodyPosition;

    public BodyDefenseEquipment(IItemType itemType, Location location)
        : base(itemType, location)
    {
    }

    protected override string PartialInspectionText
    {
        get
        {
            if (Armor > 0) return $"Arm: {Armor}";
            return Defense > 0 ? $"Def: {Defense}" : string.Empty;
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
    public virtual void OnMoved(IThing to)
    {
    }

    public static bool IsApplicable(IItemType type)
    {
        return type?.Group is ItemGroup.BodyDefenseEquipment;
    }
}