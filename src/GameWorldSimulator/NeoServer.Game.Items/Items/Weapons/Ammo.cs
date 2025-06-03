using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Combat;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types.Body;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Parsers;
using NeoServer.Game.Items.Bases;

namespace NeoServer.Game.Items.Items.Weapons;

public class Ammo : CumulativeEquipment, IAmmo
{
    public Ammo(IItemType type, Location location, IDictionary<ItemAttribute, IConvertible> attributes) : base(type,
        location, attributes)
    {
        WeaponAttack = new WeaponAttack(Metadata);
    }

    public Ammo(IItemType type, Location location, byte amount) : base(type, location, amount)
    {
        WeaponAttack = new WeaponAttack(Metadata);
    }

    protected override string PartialInspectionText
    {
        get
        {
            var elementalDamageText = WeaponAttack.ElementalDamage.AttackPower > 0
                ? $" + {WeaponAttack.ElementalDamage.AttackPower} {DamageTypeParser.Parse(WeaponAttack.ElementalDamage.DamageType)}"
                : string.Empty;

            return $"Atk: {Attack}{elementalDamageText}";
        }
    }

    public byte Attack => Metadata.Attributes.GetAttribute<byte>(ItemAttribute.Attack);

    public override bool CanBeDressed(IPlayer player)
    {
        if (Guard.IsNullOrEmpty(Vocations)) return true;

        foreach (var vocation in Vocations)
            if (vocation == player.VocationType)
                return true;

        return false;
    }

    public byte ExtraHitChance => Metadata.Attributes.GetAttribute<byte>(ItemAttribute.HitChance);
    public AmmoType AmmoType => Metadata.AmmoType;
    public ShootType ShootType => Metadata.ShootType;
    public bool HasElementalDamage => WeaponAttack.ElementalDamage.AttackPower is not 0;

    public void Throw()
    {
        Reduce();
    }

    public static bool IsApplicable(IItemType type)
    {
        return type.Group is ItemGroup.Ammo;
    }

    public void OnMoved(IThing to)
    {
    }

    public WeaponAttack WeaponAttack { get; }
}