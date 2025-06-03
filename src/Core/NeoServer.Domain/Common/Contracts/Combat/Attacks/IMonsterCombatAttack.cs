using NeoServer.Domain.Common.Combat.Structs;

namespace NeoServer.Domain.Common.Contracts.Combat.Attacks;

public interface IMonsterCombatAttack
{
    public byte AttackChance { get; set; }
    public uint Interval { get; set; }
    public CombatParameter CombatParameter { get; set; }
    public bool HasTarget { get; set; }
    Guid Id { get; }
}