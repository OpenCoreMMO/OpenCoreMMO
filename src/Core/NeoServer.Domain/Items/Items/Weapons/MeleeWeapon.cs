using System.Collections;
using NeoServer.Domain.Combat.Attacks.Obsoletes;
using NeoServer.Domain.Common.Combat;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types.Body;
using NeoServer.Domain.Common.Contracts.Items.Types.Usable;
using NeoServer.Domain.Common.Contracts.Items.Weapons.Attributes;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Parsers;
using NeoServer.Domain.Items.Bases;

namespace NeoServer.Domain.Items.Items.Weapons;

public class MeleeWeapon : Equipment, IWeapon, IUsableOnItem, IHasAttack, IHasDefense
{
    public MeleeWeapon(
        IItemType itemType,
        Location location,
        IDictionary<ItemAttribute, IConvertible> itemAttributes = null) : base(itemType, location)
    {
        //AllowedVocations  todo
        WeaponAttack = new WeaponAttack(itemType, itemAttributes);
    }

    protected override string PartialInspectionText
    {
        get
        {
            var extraDefenseText = ExtraDefense > 0 ? $" +{ExtraDefense}" :
                ExtraDefense < 0 ? $" -{ExtraDefense}" : string.Empty;

            var elementalDamageText = WeaponAttack.ElementalDamage.AttackPower > 0
                ? $" + {WeaponAttack.ElementalDamage.AttackPower} {DamageTypeParser.Parse(WeaponAttack.ElementalDamage.DamageType)},"
                : ",";

            return $"Atk: {WeaponAttack.AttackPower}{elementalDamageText} Def: {Defense}{extraDefenseText}";
        }
    }

    public WeaponAttack WeaponAttack { get; } //todo: rename to Attack

    public virtual bool CanUseOn(ushort[] items, IItem onItem)
    {
        return items is not null && ((IList)items).Contains(onItem.Metadata.ServerId);
    }

    public virtual bool CanUseOn(IItem onItem)
    {
        var useOnItems = Metadata.OnUse?.GetAttributeArray<ushort>(ItemTypeAttribute.UseOn);

        return useOnItems is not null && ((IList)useOnItems).Contains(onItem.Metadata.ServerId);
    }

    public ushort? MinHitChance { get; }

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

    private bool CalculateRegularAttack(IPlayer player, ICombatActor enemy, ushort maxDamage, out CombatDamage damage)
    {
        damage = new CombatDamage();
        if (WeaponAttack.AttackPower <= 0) return false;

        var combat = new CombatAttackValue(player.MinimumAttackPower,
            maxDamage, DamageType.Melee);

        return MeleeCombatAttack.CalculateAttack(player, enemy, combat, out damage);
    }

    private bool CalculateElementalAttack(IPlayer player, ICombatActor enemy, ushort maxDamage, out CombatDamage damage)
    {
        damage = new CombatDamage();

        if (WeaponAttack.ElementalDamage.AttackPower == 0) return false;

        var combat =
            new CombatAttackValue(player.MinimumAttackPower, maxDamage, WeaponAttack.ElementalDamage.DamageType);

        return MeleeCombatAttack.CalculateAttack(player, enemy, combat, out damage);
    }

    public static bool IsApplicable(IItemType type)
    {
        return type.Group is ItemGroup.MeleeWeapon;
    }
}