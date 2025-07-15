using NeoServer.Domain.Combat.Attacks.Obsoletes;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Items.Types.Body;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Item;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Common.Parsers;
using NeoServer.Domain.Items.Bases;

namespace NeoServer.Domain.Items.Items.Weapons;

public class MagicWeapon : Equipment, IDistanceWeapon
{
    public MagicWeapon(IItemType type, Location location) : base(type, location)
    {
    }

    private ShootType ShootType => Metadata.ShootType;

    private DamageType DamageType => Metadata.Attributes.HasAttribute(ItemTypeAttribute.Damage)
        ? Metadata.DamageType
        : ShootType.ToDamageType();

    private ushort MaxDamage => Metadata.Attributes.GetAttribute<byte>(ItemTypeAttribute.MaxHitChance);

    protected override string PartialInspectionText => string.Empty;
    public ushort MaxHitChance => Metadata.Attributes.GetAttribute<byte>(ItemTypeAttribute.MaxHitChance);
    public ushort ManaConsumption => Metadata.Attributes?.GetAttribute<ushort>(ItemTypeAttribute.ManaUse) ?? 0;
    public ushort? MinHitChance => (ushort)(MaxHitChance / 2);
    public WeaponType WeaponType => WeaponType.Magical;

    public bool Attack(ICombatActor actor, ICombatActor enemy, out CombatAttackResult combatResult)
    {
        combatResult = new CombatAttackResult(ShootType);

        if (actor is not IPlayer player) return false;
        if (!player.HasEnoughMana(ManaConsumption)) return false;

        var combat = new CombatAttackValue((ushort)(MaxDamage / 2), MaxDamage, Range, DamageType);

        if (DistanceCombatAttack.CalculateAttack(actor, enemy, combat, out var damage))
        {
            player.DecreaseMana(ManaConsumption);
            enemy.TakeDamage(actor, damage);
            return true;
        }

        return false;
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

    public static bool IsApplicable(IItemType type)
    {
        return type.Group is ItemGroup.MagicWeapon;
    }
}