using NeoServer.Domain.Common.Contracts;

namespace NeoServer.Domain.Creatures.Monster.Summon;

public class MonsterAttackCooldown(Guid attackId, uint duration) : IHasCooldown
{
    public Guid CooldownId { get; } = attackId;
    public (string Name, uint Cooldown) PrimaryGroup { get; set; }
    public (string Name, uint Cooldown) SecondaryGroup { get; set; }
    public uint Cooldown { get; set; } = duration;
}