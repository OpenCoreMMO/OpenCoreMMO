﻿using System.Text;
using NeoServer.Game.Combat.Attacks;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Weapons;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Common.Parsers;
using NeoServer.Game.Item.Bases;

namespace NeoServer.Game.Item.Items.Weapons;

public class ThrowableWeapon : CumulativeEquipment, IThrowableWeapon
{
    public ThrowableWeapon(IItemType type, Location location,
        IDictionary<ItemAttribute, IConvertible> attributes) : base(type, location, attributes)
    {
        WeaponAttack = new WeaponAttack(Metadata);
    }

    public ThrowableWeapon(IItemType type, Location location, byte amount) : base(type, location, amount)
    {
    }
    
    public WeaponAttack WeaponAttack { get; } //todo: rename to Attack

    public byte ExtraHitChance => Metadata.Attributes.GetAttribute<byte>(ItemAttribute.HitChance);
    private byte Defense => Metadata.Attributes.GetAttribute<byte>(ItemAttribute.Defense);
    private decimal BreakChance => Metadata.Attributes.GetAttribute<decimal>("breakChance");

    protected override string PartialInspectionText
    {
        get
        {
            var range = Range > 0 ? $"Range: {Range}" : string.Empty;
            var hit = ExtraHitChance > 0 ? $"Hit% +{ExtraHitChance}" : string.Empty;
            var elementalDamageText =  WeaponAttack.ElementalDamage.AttackPower > 0
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

    public override bool CanBeDressed(IPlayer player)
    {
        if (Guard.IsNullOrEmpty(Vocations)) return true;

        foreach (var vocation in Vocations)
            if (vocation == player.VocationType)
                return true;

        return false;
    }

    public byte Range => Metadata.Attributes.GetAttribute<byte>(ItemAttribute.Range);

    public bool Attack(ICombatActor actor, ICombatActor enemy, out CombatAttackResult combatResult)
    {
        // combatResult = new CombatAttackResult(Metadata.ShootType);
        //
        // if (actor is not IPlayer player) return false;
        //
        // var maxDamage = player.MaximumAttackPower;
        // var combat = new CombatAttackValue(actor.MinimumAttackPower, maxDamage, Range, DamageType.Physical);
        //
        // if (!DistanceCombatAttack.CanAttack(actor, enemy, combat)) return false;
        //
        // if (BreakChance > 0 && GameRandom.Random.Next(1, maxValue: 100) <= BreakChance) Reduce();
        //
        // var hitChance =
        //     (byte)(DistanceHitChanceCalculation.CalculateFor1Hand(player.GetSkillLevel(player.SkillInUse), Range) +
        //            ExtraHitChance);
        // var missed = DistanceCombatAttack.MissedAttack(hitChance);
        //
        // if (missed)
        // {
        //     combatResult.Missed = true;
        //     return true;
        // }
        //
        // if (!DistanceCombatAttack.CalculateAttack(actor, enemy, combat, out var damage)) return false;
        //
        // //enemy.ReceiveAttackFrom(actor, damage);
        //
        // return true;
        throw new NotImplementedException();
    }
    public bool ShouldBreak() => BreakChance > 0 && GameRandom.Random.Next(1, maxValue: 100) <= BreakChance;
    public static bool IsApplicable(IItemType type)
    {
        return type.Group is ItemGroup.ThrowableDistanceWeapon;
    }

    public void OnMoved(IThing to)
    {
    }
}