using System.Text;
using NeoServer.Game.Combat.Attacks;
using NeoServer.Game.Combat.Calculations;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Items.Types.Body;
using NeoServer.Game.Common.Contracts.Items.Weapons.Attributes;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Items.Bases;

namespace NeoServer.Game.Items.Items.Weapons;

public class DistanceWeapon(IItemType type, Location location): Equipment(type, location), IDistanceWeapon, IHasAttackBonus, INeedsAmmo
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

    public override bool CanBeDressed(IPlayer player)
    {
        if (Guard.IsNullOrEmpty(Vocations)) return true;

        foreach (var vocation in Vocations)
            if (vocation == player.VocationType)
                return true;

        return false;
    }

    public byte ExtraAttack => Metadata.Attributes.GetAttribute<byte>(ItemAttribute.Attack);
    public sbyte ExtraHitChance => Metadata.Attributes.GetAttribute<sbyte>(ItemAttribute.HitChance);
    public byte Range => Metadata.Attributes.GetAttribute<byte>(ItemAttribute.Range);
    public byte AttackBonus => Metadata.Attributes.GetAttribute<byte>(ItemAttribute.Attack);
    public bool CanShootAmmunition(IAmmo ammo) => Metadata.AmmoType == (ammo?.AmmoType ?? AmmoType.None);

    public ushort? MinHitChance { get; }

    public bool Attack(ICombatActor actor, ICombatActor enemy, out CombatAttackResult combatResult)
    {
        var result = false;
        combatResult = new CombatAttackResult();

        if (actor is not IPlayer player) return false;

        if (player.Inventory[Slot.Ammo] is not IAmmo ammo) return false;

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

    public static bool IsApplicable(IItemType type)
    {
        return type.Group is ItemGroup.DistanceWeapon;
    }

    private void UseElementalDamage(ICombatActor actor, ICombatActor enemy, ref CombatAttackResult combatResult,
        ref bool result, IPlayer player, IAmmo ammo, ref ushort maxDamage, ref CombatAttackValue combat)
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