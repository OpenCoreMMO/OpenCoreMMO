using NeoServer.Domain.Common.Contracts;

namespace NeoServer.Domain.Creatures.Monster.Summon;

public class MonsterAttackCooldown(Guid attackId, uint duration) : IHasCooldown
{
    public Guid CooldownId { get; } = attackId;
    public (int Id, uint Cooldown) PrimaryGroup { get; }
    public (int Id, uint Cooldown) SecondaryGroup { get; }
    public uint Cooldown { get; } = duration;
}