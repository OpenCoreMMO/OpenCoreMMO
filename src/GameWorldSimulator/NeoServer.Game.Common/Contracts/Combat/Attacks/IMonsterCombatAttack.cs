using System;
using NeoServer.Game.Common.Combat.Structs;
using NeoServer.Game.Common.Creatures.Structs;
using NeoServer.Game.Common.Item;

namespace NeoServer.Game.Common.Contracts.Combat.Attacks;

public interface IMonsterCombatAttack
{
    public byte AttackChance { get; set; }
    public uint Interval { get; set; }
    public CombatParameter CombatParameter { get; set; }
    public bool HasTarget { get; set; }
    Guid Id { get; }
}