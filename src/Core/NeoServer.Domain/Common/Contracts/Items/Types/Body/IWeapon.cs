using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures.Players;
using NeoServer.Domain.Common.Item;

namespace NeoServer.Domain.Common.Contracts.Items.Types.Body;

public delegate bool AttackEnemy(ICombatActor actor, ICombatActor enemy, DamageType damageType, int minDamage,
    int maxDamage, out CombatDamage damage);

public interface IWeapon : IBodyEquipmentEquipment
{
    bool TwoHanded => Metadata.BodyPosition == Slot.TwoHanded;

    new Slot Slot => Slot.Left;
    public WeaponType Type => Metadata.WeaponType;
    public ushort? MinHitChance { get; }
    bool Attack(ICombatActor actor, ICombatActor enemy, out CombatAttackResult combat);
}