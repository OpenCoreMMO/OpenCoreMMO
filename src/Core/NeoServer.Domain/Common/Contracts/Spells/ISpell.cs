using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Results;

namespace NeoServer.Domain.Common.Contracts.Spells;

public delegate void InvokeSpell(ICombatActor creature, ISpell spell);

public interface ISpell : IHasCooldown
{
    EffectT Effect { get; }

    string Name { get; set; }
    bool ShouldSay { get; }
    string Words { get; set; }
    bool Enabled { get; }
    bool BlockWalls { get; set; }
    ushort ManaConsumption { get; set; }
    ushort SoulConsumption { get; set; }
    bool NeedDirection { get; }
    bool CasterNeedsTargetOrDirection { get; }
    bool NeedsTarget { get; set; }
    byte? Range { get; }
    bool IsAggressive { get; }
    bool BlockingCreature { get; set; }
    bool BlockingSolid { get; set; }
    bool NeedWeapon { get; }
    bool NeedLearn { get; }

    byte[] VocationIds { get; set; }
    string[] Vocations { get; }
    bool NeedsPremium { get; }
    ushort MinLevel { get; }
    ushort MinMagicLevel { get; }
    Result Invoke(ICombatActor actor, IThing target, bool isHotkey);
    Result CanCast(ICombatActor caster, IThing target);
}

public interface ICommandSpell : ISpell
{
    public object[] Params { get; set; }
}