using System;
using System.Linq;
using NeoServer.Game.Combat.Conditions;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Spells;
using NeoServer.Game.Common.Creatures;

namespace NeoServer.Game.Combat.Spells;

public abstract class BaseSpell : ISpell
{
    public abstract uint Duration { get; }

    public abstract ConditionType ConditionType { get; }
    public virtual string Name { get; set; }
    public abstract EffectT Effect { get; }
    public virtual ushort Mana { get; set; }
    public virtual ushort MinLevel { get; set; } = 0;
    public virtual byte[] Vocations { get; set; }
    public uint Cooldown { get; set; }

    public bool InvokeOn(ICombatActor actor, ICombatActor onCreature, string words, out InvalidOperation error)
    {
        if (!CanBeUsedBy(actor, out error)) return false;
       
        if (!onCreature.HasCondition(ConditionType))
            if (!OnCast(onCreature, words, out error))
                return false;

        AddCondition(onCreature);

        if (actor is IPlayer) AddCooldown(actor);
        
        OnSpellInvoked?.Invoke(onCreature, this);
        if (actor is IPlayer player) player.ConsumeMana(Mana);
        
        return true;
    }

    public bool Invoke(ICombatActor actor, string words, out InvalidOperation error)
    {
        if (!CanBeUsedBy(actor, out error)) return false;
        
        if (!actor.HasCondition(ConditionType))
            if (!OnCast(actor, words, out error))
                return false;

        AddCondition(actor);

        if (actor is IPlayer) AddCooldown(actor);

        if (actor is IPlayer player) player.ConsumeMana(Mana);
        OnSpellInvoked?.Invoke(actor, this);
        return true;
    }

    public virtual bool ShouldSay { get; }

    public static event InvokeSpell OnSpellInvoked;

    public abstract bool OnCast(ICombatActor actor, string words, out InvalidOperation error);

    public bool CanBeUsedBy(ICombatActor actor, out InvalidOperation error)
    {
        error = InvalidOperation.None;

        if (actor is IPlayer player)
        {
            if (!player.HasEnoughMana(Mana))
            {
                error = InvalidOperation.NotEnoughMana;
                return false;
            }

            if (!player.HasEnoughLevel(MinLevel))
            {
                error = InvalidOperation.NotEnoughLevel;
                return false;
            }

            if (!Vocations?.Contains(player.VocationType) ?? false)
            {
                error = InvalidOperation.VocationCannotUseSpell;
                return false;
            }

            if (!player.SpellCooldownHasExpired(this) || !player.CooldownHasExpired(CooldownType.Spell))
            {
                error = InvalidOperation.Exhausted;
                return false;
            }
        }

        return true;
    }

    public virtual void OnEnd(ICombatActor actor)
    {
    }

    public virtual void AddCondition(ICombatActor actor)
    {
        if (ConditionType == ConditionType.None) return;

        if (actor.HasCondition(ConditionType, out var existingCondition))
        {
            actor.AddCondition(existingCondition);
            return;
        }

        var condition = new Condition(ConditionType, Duration);
        condition.EndAction = () => OnEnd(actor);

        actor.AddCondition(condition);
    }

    private void AddCooldown(ICombatActor actor)
    {
        actor.StartSpellCooldown(this);
    }
}

public abstract class Spell<T> : BaseSpell where T : ISpell
{
    private static readonly Lazy<T> Lazy = new(() => (T)Activator.CreateInstance(typeof(T), true));
    public override bool ShouldSay => true;
    public string Words { get; set; }
    public static T Instance => Lazy.Value;
}