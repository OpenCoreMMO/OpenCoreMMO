using System.Text;
using NeoServer.Domain.Combat.Attacks;
using NeoServer.Domain.Combat.Calculations;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types.Body;
using NeoServer.Domain.Common.Contracts.Items.Weapons.Attributes;
using NeoServer.Domain.Common.Creatures.Players;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Bases;

namespace NeoServer.Domain.Items.Items.Weapons;

public class DistanceWeapon(IItemType type, Location location)
    : Equipment(type, location), IDistanceWeapon, IHasAttackBonus, INeedsAmmo
{
    protected override string PartialInspectionText
    {
        get
        {
            var range = Range > 0 ? $"Range: {Range}" : string.Empty;
            var atk = ExtraAttack > 0 ? $"Atk: {ExtraAttack:+#}" : string.Empty;
            var hit = ExtraHitChance != 0 ? $"Hit% {ExtraHitChance:+#;-#}" : string.Empty;

            if (Guard.AllNullOrEmpty(range, atk, hit)) return string.Empty;

            var stringBuilder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(range)) stringBuilder.Append($"{range}, ");
            if (!string.IsNullOrWhiteSpace(atk)) stringBuilder.Append($"{atk}, ");
            if (!string.IsNullOrWhiteSpace(hit)) stringBuilder.Append($"{hit}, ");

            stringBuilder.Remove(stringBuilder.Length - 2, 2);
            return stringBuilder.ToString();
        }
    }

    public byte ExtraAttack => Metadata.Attributes.GetAttribute<byte>(ItemAttribute.Attack);

    public override bool CanBeDressed(IPlayer player)
    {
        if (Guard.IsNullOrEmpty(Vocations)) return true;

        foreach (var vocation in Vocations)
            if (vocation == player.VocationType)
                return true;

        return false;
    }

    public sbyte ExtraHitChance => Metadata.Attributes.GetAttribute<sbyte>(ItemAttribute.HitChance);
    public byte Range => Metadata.Attributes.GetAttribute<byte>(ItemAttribute.Range);

    public ushort? MinHitChance { get; }

    public bool Attack(ICombatActor actor, ICombatActor enemy, out CombatAttackResult combatResult)
    {
        var result = false;
        combatResult = new CombatAttackResult();

        if (actor is not IPlayer player) return false;

        if (player.Inventory[Slot.Ammo] is not Ammo ammo) return false;

        if (ammo.AmmoType != Metadata.AmmoType) return false;

        if (ammo.Amount <= 0) return false;

        if (!DistanceCombatAttack.CanAttack(actor, enemy, Range)) return false;

        var distance = (byte)actor.Location.GetSqmDistance(enemy.Location);

        var hitChance =
            (byte)(DistanceHitChanceCalculation.CalculateFor2Hands(player.GetSkillLevel(player.SkillInUse), distance) +
                   ExtraHitChance);

        combatResult.ShootType = ammo.ShootType;

        var missed = DistanceCombatAttack.MissedAttack(hitChance);

        if (missed)
        {
            combatResult.Missed = true;
            ammo.Throw();
            return true;
        }

        var maxDamage = player.CalculateAttackPower(0.09f, (ushort)(ammo.Attack + ExtraAttack));

        var combat = new CombatAttackValue(actor.MinimumAttackPower, maxDamage, Range, DamageType.Physical);

        if (DistanceCombatAttack.CalculateAttack(actor, enemy, combat, out var damage))
        {
            enemy.TakeDamage(actor, damage);
            result = true;
        }

        UseElementalDamage(actor, enemy, ref combatResult, ref result, player, ammo, ref maxDamage, ref combat);

        if (result) ammo.Throw();

        return result;
    }

    public void OnMoved(IThing to)
    {
    }

    public byte AttackBonus => Metadata.Attributes.GetAttribute<byte>(ItemAttribute.Attack);

    public bool CanShootAmmunition(Ammo ammo)
    {
        return Metadata.AmmoType == (ammo?.AmmoType ?? AmmoType.None);
    }

    public static bool IsApplicable(IItemType type)
    {
        return type.Group is ItemGroup.DistanceWeapon;
    }

    private void UseElementalDamage(ICombatActor actor, ICombatActor enemy, ref CombatAttackResult combatResult,
        ref bool result, IPlayer player, Ammo ammo, ref ushort maxDamage, ref CombatAttackValue combat)
    {
        if (!ammo.HasElementalDamage) return;

        maxDamage = 100; //player.CalculateAttackPower(0.09f, (ushort)(ammo.ElementalDamage.Item2 + ExtraAttack)); //TODO
        combat = new CombatAttackValue(actor.MinimumAttackPower, maxDamage, Range,
            ammo.WeaponAttack.ElementalDamage.DamageType);

        if (!DistanceCombatAttack.CalculateAttack(actor, enemy, combat, out var elementalDamage)) return;

        combatResult.DamageType = ammo.WeaponAttack.ElementalDamage.DamageType;

        //enemy.ReceiveAttackFrom(actor, elementalDamage);
        result = true;
    }
}