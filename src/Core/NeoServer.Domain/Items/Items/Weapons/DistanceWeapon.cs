using System.Text;
using NeoServer.Domain.Combat.Attacks.Obsoletes;
using NeoServer.Domain.Common.Combat;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types.Body;
using NeoServer.Domain.Common.Contracts.Items.Weapons.Attributes;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Items.Bases;

namespace NeoServer.Domain.Items.Items.Weapons;

public class DistanceWeapon(IItemType type, Location location)
    : Equipment(type, location), IDistanceWeapon, IHasAttackBonus, INeedsAmmo
{
    public WeaponAttack WeaponAttack { get; }

    protected override string PartialInspectionText
    {
        get
        {
            var range = Range > 0 ? $"Range: {Range}" : string.Empty;
            var atk = AttackPower > 0 ? $"Atk: {AttackPower:+#}" : string.Empty;
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