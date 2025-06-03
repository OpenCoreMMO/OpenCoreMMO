using System;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Contracts.Combat.Attacks;
using NeoServer.Game.Common.Creatures.Structs;
using NeoServer.Game.Common.Item;

namespace NeoServer.Game.Combat.Attacks;

public class MonsterCombatAttack : IMonsterCombatAttack
{
    public Guid Id { get; } = Guid.NewGuid();
    public byte AttackChance { get; set; }
    public uint Interval { get; set; }
    public CombatParameter CombatParameter { get; set; }
    public bool HasTarget { get; set; }
}