using System.Collections;
using NeoServer.Game.Combat.Attacks;
using NeoServer.Game.Common.Combat;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types.Body;
using NeoServer.Game.Common.Contracts.Items.Types.Usable;
using NeoServer.Game.Common.Contracts.Items.Weapons.Attributes;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Parsers;
using NeoServer.Game.Items.Bases;

namespace NeoServer.Game.Items.Items.Weapons;

public class MeleeWeapon : Equipment, IWeapon, IUsableOnItem, IHasAttack, IHasDefense
{
    public MeleeWeapon(IItemType itemType, Location location) : base(itemType, location)
    {
        //AllowedVocations  todo
        WeaponAttack = new WeaponAttack(Metadata);
    }

    protected override string PartialInspectionText
    {
        get
        {
            var defense = Metadata.Attributes.GetAttribute<byte>(ItemAttribute.Defense);
            var extraDefense = Metadata.Attributes.GetAttribute<sbyte>(ItemAttribute.ExtraDefense);

            var extraDefenseText = extraDefense > 0 ? $" +{extraDefense}" :
                extraDefense < 0 ? $" -{extraDefense}" : string.Empty;

            var elementalDamageText = WeaponAttack.ElementalDamage.AttackPower > 0
                ? $" + {WeaponAttack.ElementalDamage.AttackPower} {DamageTypeParser.Parse(WeaponAttack.ElementalDamage.DamageType)},"
                : ",";

            return $"Atk: {WeaponAttack.AttackPower}{elementalDamageText} Def: {defense}{extraDefenseText}";
        }
    }

    public sbyte ExtraDefense => Metadata.Attributes.GetAttribute<sbyte>(ItemAttribute.ExtraDefense);
    public WeaponAttack WeaponAttack { get; }//todo: rename to attack
    public byte Defense => Metadata.Attributes.GetAttribute<byte>(ItemAttribute.Defense);
    public virtual bool CanUseOn(ushort[] items, IItem onItem)
    {
        return items is not null && ((IList)items).Contains(onItem.Metadata.ServerId);
    }

    public virtual bool CanUseOn(IItem onItem)
    {
        var useOnItems = Metadata.OnUse?.GetAttributeArray<ushort>(ItemAttribute.UseOn);

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

    public ushort AttackPower => Metadata.Attributes.GetAttribute<ushort>(ItemAttribute.Attack);
    public ushort? MinHitChance { get; }

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

        var combat = new CombatAttackValue(player.MinimumAttackPower, maxDamage, WeaponAttack.ElementalDamage.DamageType);

        return MeleeCombatAttack.CalculateAttack(player, enemy, combat, out damage);
    }

    public static bool IsApplicable(IItemType type)
    {
        return type.Group is ItemGroup.MeleeWeapon;
    }
}