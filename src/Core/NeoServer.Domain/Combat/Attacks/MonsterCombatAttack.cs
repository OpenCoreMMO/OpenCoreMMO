using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Combat.Attacks;

namespace NeoServer.Domain.Combat.Attacks;

public class MonsterCombatAttack : IMonsterCombatAttack
{
    public Guid Id { get; } = Guid.NewGuid();
    public byte AttackChance { get; set; }
    public uint Interval { get; set; }
    public CombatParameter CombatParameter { get; set; }
    public bool HasTarget { get; set; }
}