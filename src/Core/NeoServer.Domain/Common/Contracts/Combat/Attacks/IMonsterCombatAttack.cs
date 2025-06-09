using NeoServer.Domain.Common.Combat.Structs;
using NeoServer.Domain.Common.Contracts.Spells;

namespace NeoServer.Domain.Common.Contracts.Combat.Attacks;

public interface IMonsterCombatAttack
{
    public byte AttackChance { get; set; }
    public uint Interval { get; set; }
    public CombatParameter CombatParameter { get; set; }
    public bool NeedTarget { get; set; }
    Guid Id { get; }
    ISpell Spell { get; set; }
}