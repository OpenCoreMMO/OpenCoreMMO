using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Spell;

namespace NeoServer.Game.Common.Contracts.Spells;

public delegate void InvokeSpell(ICombatActor creature, ISpell spell);

public interface ISpell
{
    EffectT Effect { get; }
    ushort Mana { get; set; }
    ushort MinLevel { get; set; }
    string Name { get; set; }
    uint Cooldown { get; set; }
    bool ShouldSay { get; }
    byte[] VocationIds { get; set; }
    string[] Vocations { get; }
    MagicGroup[] Groups { get; }
    uint[] GroupCooldown { get; }

    /// <summary>
    ///     Indicates if should train magic level when spell is cast
    /// </summary>
    bool IncreaseSkill => true;

    string Words { get; set; }
    bool Enabled { get; }
    bool BlockWalls { get; }
    bool NeedsTarget { get; }
    bool NeedWeapon { get; }
    bool NeedLearn { get; }
    bool NeedDirection { get; }
    bool CasterNeedsTargetOrDirection { get; }

    bool Invoke(ICombatActor actor, string words, out InvalidOperation error);
    bool InvokeOn(ICombatActor actor, ICombatActor onCreature, string words, out InvalidOperation error);
    bool CanBeCastBy(ICombatActor caster, out InvalidOperation error);
}

public interface ICommandSpell : ISpell
{
    public object[] Params { get; set; }
    bool ISpell.IncreaseSkill => false;
}