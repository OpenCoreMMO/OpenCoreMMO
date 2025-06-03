using NeoServer.Domain.Combat.Spells;
using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Item;

namespace NeoServer.Domain.Combat.Attacks;

public class SpeedCombatAttack : DistanceCombatAttack
{
    public SpeedCombatAttack(uint duration, short speedChance, byte range, ShootType shootType) : base(range,
        shootType)
    {
        SpeedChange = speedChance;

        if (SpeedChange < -1000)
            SpeedChange = -10000;

        Duration = duration;
    }

    public uint Duration { get; } = 10000;

    public short SpeedChange { get; set; }

    public override bool TryAttack(ICombatActor actor, ICombatActor enemy, CombatAttackValue option,
        out CombatAttackResult combatResult)
    {
        combatResult = new CombatAttackResult(option.DamageType);

        if (CalculateAttack(actor, enemy, option, out var damage))
        {
            var invokeResult = SpeedChange > 0
                ? HasteSpell.Instance.Invoke(actor, enemy, false)
                : ParalyzeSpell.Instance.Invoke(actor, enemy, false);

            if (invokeResult.Succeeded) return true;
        }

        return false;
    }
}