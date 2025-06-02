using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Item;
using NeoServer.Game.Common.Location.Structs;

namespace NeoServer.Game.Combat.Attacks;

public class DistanceCombatAttack : CombatAttack
{
    public DistanceCombatAttack(byte range, ShootType shootType)
    {
        Range = range;
        ShootType = shootType;
    }

    public byte Range { get; }
    public ShootType ShootType { get; }

    public static bool CanAttack(ICombatActor actor, ICombatActor enemy, CombatAttackValue option)
    {
        var targetLocation = GetTargetLocation(actor, enemy);

        return actor.Location.GetMaxSqmDistance(targetLocation) <= option.Range;
    }

    public static bool CanAttack(ICombatActor actor, ICombatActor enemy, byte maxRange)
    {
        var targetLocation = GetTargetLocation(actor, enemy);

        return actor.Location.GetMaxSqmDistance(targetLocation) <= maxRange;
    }

    public static bool CalculateAttack(ICombatActor actor, ICombatActor enemy, CombatAttackValue option,
        out CombatDamage damage)
    {
        damage = new CombatDamage();
        if (!CanAttack(actor, enemy, option)) return false;

        var damageValue = (ushort)GameRandom.Random.NextInRange(option.MinDamage, option.MaxDamage);

        damage = new CombatDamage(damageValue, option.DamageType);

        return true;
    }

    public static bool MissedAttack(byte hitChance)
    {
        var value = GameRandom.Random.Next(1, maxValue: 100);
        return hitChance < value;
    }

    public override bool TryAttack(ICombatActor actor, ICombatActor enemy, CombatAttackValue option,
        out CombatAttackResult combatResult)
    {
        combatResult = new CombatAttackResult(ShootType)
        {
            EffectT = option.DamageEffect
        };

        if (!CalculateAttack(actor, enemy, option, out var damage)) return false;

        combatResult.DamageType = option.DamageType;

        var targetLocation = GetTargetLocation(actor, enemy);

        if (enemy is null)
        {
            var area = new[] { new AffectedLocation(targetLocation.Translate()) };
            combatResult.Area = area;
            actor.PropagateAttack(area, damage);
        }

        enemy?.TakeDamage(actor, damage);
        return true;
    }

    private static Location GetTargetLocation(ICombatActor actor, ICombatActor enemy)
    {
        return enemy?.Location ?? actor.Location.GetNextLocation(actor.Direction);
    }
}