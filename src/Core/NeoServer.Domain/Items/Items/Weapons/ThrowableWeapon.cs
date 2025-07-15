using System.Text;
using NeoServer.Domain.Combat.Attacks.Obsoletes;
using NeoServer.Domain.Combat.Calculations;
using NeoServer.Domain.Common.Combat;
using NeoServer.Domain.Common.Combat.Structs;
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

public class ThrowableWeapon : CumulativeEquipment, IWeapon, IHasAttack, IHasRange
{
    private decimal BreakChance => Metadata.Attributes.HasCustomAttribute("breakChance")
        ? Metadata.Attributes.GetCustomAttribute<decimal>("breakChance")
        : 100;

    public bool ShouldBreak => BreakChance > 0 && GameRandom.Random.Next(1, maxValue: 100) <= BreakChance;
    public WeaponAttack WeaponAttack { get; } //todo: rename to Attack

    public ushort? MinHitChance { get; }

    protected override string PartialInspectionText
    {
        get
        {
            var range = Range > 0 ? $"Range: {Range}" : string.Empty;
            var hit = ExtraHitChance > 0 ? $"Hit% +{ExtraHitChance}" : string.Empty;
            var elementalDamageText = WeaponAttack.ElementalDamage.AttackPower > 0
                ? $" + {WeaponAttack.ElementalDamage.AttackPower} {DamageTypeParser.Parse(WeaponAttack.ElementalDamage.DamageType)},"
                : ",";

            var stringBuilder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(range)) stringBuilder.Append($"{range}, ");

            stringBuilder.Append($"Atk: {WeaponAttack.AttackPower}{elementalDamageText} ");
            stringBuilder.Append($"Def: {Defense}, ");

            if (!string.IsNullOrWhiteSpace(hit)) stringBuilder.Append($"{hit}, ");

            stringBuilder.Remove(stringBuilder.Length - 2, 2);

            return stringBuilder.ToString();
        }
    }

    public ThrowableWeapon(
        IItemType itemType,
        Location location,
        IDictionary<ItemTypeAttribute, IConvertible> itemTypeAttributes,
        IDictionary<ItemAttribute, IConvertible> itemAttributes) : base(itemType, location, itemTypeAttributes)
    {
        WeaponAttack = new WeaponAttack(itemType, itemAttributes);
    }

    public ThrowableWeapon(IItemType itemType, Location location, byte amount) : base(itemType, location, amount)
    {
        WeaponAttack = new WeaponAttack(itemType);
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
        combatResult = new CombatAttackResult(Metadata.ShootType);

        if (actor is not IPlayer player) return false;

        var maxDamage = player.CalculateAttackPower(0.09f, AttackPower);
        var combat = new CombatAttackValue(actor.MinimumAttackPower, maxDamage, Range, DamageType.Physical);

        if (!DistanceCombatAttack.CanAttack(actor, enemy, combat)) return false;

        if (BreakChance > 0 && GameRandom.Random.Next(1, maxValue: 100) <= BreakChance) Reduce();

        var hitChance =
            (byte)(DistanceHitChanceCalculation.CalculateFor1Hand(player.GetSkillLevel(player.SkillInUse), Range) +
                   ExtraHitChance);
        var missed = DistanceCombatAttack.MissedAttack(hitChance);

        if (missed)
        {
            combatResult.Missed = true;
            return true;
        }

        if (!DistanceCombatAttack.CalculateAttack(actor, enemy, combat, out var damage)) return false;

        enemy.TakeDamage(actor, damage);

        return true;
    }

    public void OnMoved(IThing to)
    {
    }

    public static bool IsApplicable(IItemType type)
    {
        return type.Group is ItemGroup.ThrowableDistanceWeapon;
    }
}