using NeoServer.Domain.Common.Combat;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types.Body;
using NeoServer.Domain.Common.Contracts.Items.Weapons.Attributes;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Parsers;
using NeoServer.Domain.Items.Bases;

namespace NeoServer.Domain.Items.Items.Weapons;

public class Ammo : CumulativeEquipment, IBodyEquipmentEquipment, IHasAttack
{
    public Ammo(
        IItemType itemType,
        Location location,
        IDictionary<ItemTypeAttribute, IConvertible> itemTypeAttributes,
        IDictionary<ItemAttribute, IConvertible> itemAttributes) : base(itemType, location, itemTypeAttributes)
    {
        WeaponAttack = new WeaponAttack(itemType, itemAttributes);
    }

    public AmmoType AmmoType => Metadata.AmmoType;
    public ShootType ShootType => Metadata.ShootType;
    public bool HasElementalDamage => WeaponAttack.ElementalDamage.AttackPower is not 0;

    protected override string PartialInspectionText
    {
        get
        {
            var elementalDamageText = WeaponAttack.ElementalDamage.AttackPower > 0
                ? $" + {WeaponAttack.ElementalDamage.AttackPower} {DamageTypeParser.Parse(WeaponAttack.ElementalDamage.DamageType)}"
                : string.Empty;

            return $"Atk: {AttackPower}{elementalDamageText}";
        }
    }

    public override bool CanBeDressed(IPlayer player)
    {
        if (Guard.IsNullOrEmpty(Vocations)) return true;

        foreach (var vocation in Vocations)
            if (vocation == player.VocationType)
                return true;

        return false;
    }

    public void OnMoved(IThing to)
    {
    }

    public WeaponAttack WeaponAttack { get; }

    public void Throw()
    {
        Reduce();
    }

    public static bool IsApplicable(IItemType type)
    {
        return type.Group is ItemGroup.Ammo;
    }
}