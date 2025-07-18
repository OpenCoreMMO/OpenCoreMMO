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
    public WeaponAttack WeaponAttack { get; } //todo: rename to Attack

    public ushort? MinHitChance { get; }

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

    public MeleeWeapon(
        IItemType itemType,
        Location location,
        IDictionary<ItemAttribute, IConvertible> itemAttributes = null) : base(itemType, location)
    {
        //AllowedVocations  todo
        WeaponAttack = new WeaponAttack(itemType, itemAttributes);
    }

    public virtual bool CanUseOn(ushort[] items, IItem onItem)
    {
        return items is not null && ((IList)items).Contains(onItem.Metadata.ServerId);
    }

    public virtual bool CanUseOn(IItem onItem)
    {
        var useOnItems = Metadata.OnUse?.GetAttributeArray<ushort>(ItemTypeAttribute.UseOn);

        return useOnItems is not null && ((IList)useOnItems).Contains(onItem.Metadata.ServerId);
    }

    public override bool CanBeDressed(IPlayer player)
    {
        if (Guard.IsNullOrEmpty(Vocations)) return true;

        foreach (var vocation in Vocations)
            if (vocation == player.VocationType)
                return true;

        return false;
    }

    public bool Attack(ICombatActor actor, ICombatActor enemy, out CombatAttackResult combatResult)
    {
        combatResult = new CombatAttackResult(DamageType.Melee);

        if (actor is not IPlayer player) return false;

        var result = false;

        var attackPower = WeaponAttack.AttackPower + WeaponAttack.ElementalDamage.AttackPower;
        var maxDamage = player.MaximumAttackPower;

        if (CalculateRegularAttack(player, enemy, maxDamage, out var damage))
        {
            var attackPowerPercentageFromTotal = 100 - WeaponAttack.AttackPower * 100 / attackPower;
            var realDamage = (ushort)(damage.Damage - damage.Damage * ((double)attackPowerPercentageFromTotal / 100));

            damage.SetNewDamage(realDamage);
            //  enemy.ReceiveAttackFrom(player, damage);

            result = true;
        }

        if (CalculateElementalAttack(player, enemy, maxDamage, out var elementalDamage))
        {
            var attackPowerPercentageFromTotal = 100 - WeaponAttack.ElementalDamage.AttackPower * 100 / attackPower;
            var realDamage = (ushort)(elementalDamage.Damage -
                                      elementalDamage.Damage * ((double)attackPowerPercentageFromTotal / 100));

            elementalDamage.SetNewDamage(realDamage);
            //enemy.ReceiveAttackFrom(player, elementalDamage);

            result = true;
        }

        return result;
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