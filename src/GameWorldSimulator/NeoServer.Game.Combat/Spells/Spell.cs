using System;
using System.Linq;
using NeoServer.Game.Combat.Conditions;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Spells;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Spell;

namespace NeoServer.Game.Combat.Spells;

public abstract class BaseSpell : ISpell
{
    public abstract uint Duration { get; }
    public virtual bool NeedsTarget => false;

    public abstract ConditionType ConditionType { get; }
    public virtual string Name { get; set; }
    public abstract EffectT Effect { get; }
    public virtual ushort Mana { get; set; }
    public virtual ushort MinLevel { get; set; } = 0;
    public virtual bool NeedWeapon { get; }
    public virtual bool NeedLearn { get; }
    public virtual bool BlockWalls { get; }
    public virtual string[] Vocations { get; }
    public virtual SpellGroup[] Groups { get; }
    public virtual uint[] GroupCooldown { get; }
    public virtual byte[] VocationIds { get; set; }
    public virtual uint Cooldown { get; set; }
    public virtual bool Premium { get; }
    public virtual byte Range { get; }
    public virtual bool NeedDirection { get; }
    public virtual bool CasterNeedsTargetOrDirection { get; }

    public bool InvokeOn(ICombatActor actor, ICombatActor onCreature, string words, out InvalidOperation error)
    {
        if (!CanBeCastBy(actor, out error)) return false;

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
        error = InvalidOperation.None;

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
    public abstract string Words { get; set; }
    public virtual bool Enabled => true;

    public static event InvokeSpell OnSpellInvoked;

    public abstract bool OnCast(ICombatActor caster, string words, out InvalidOperation error);

    public bool CanBeCastBy(ICombatActor caster, out InvalidOperation error)
    {
        error = InvalidOperation.None;

        if (caster is IPlayer player)
        {
            if (!VocationIds?.Contains(player.VocationType) ?? false)
            {
                error = InvalidOperation.VocationCannotUseSpell;
                return false;
            }

            if (Premium && player.PremiumTime <= 0)
            {
                error = InvalidOperation.PremiumTimeIsRequired;
                return false;
            }

            if (!player.HasEnoughLevel(MinLevel))
            {
                error = InvalidOperation.NotEnoughLevel;
                return false;
            }

            if (NeedWeapon && !player.Inventory.IsUsingWeapon)
            {
                error = InvalidOperation.SpellNeedsWeapon;
                return false;
            }

            if (!player.HasEnoughMana(Mana))
            {
                error = InvalidOperation.NotEnoughMana;
                return false;
            }

            if (!player.SpellCooldownHasExpired(this) || !player.CooldownHasExpired(CooldownType.Spell))
            {
                error = InvalidOperation.Exhausted;
                return false;
            }
        }

        if (caster.CurrentTarget is null && NeedsTarget)
        {
            error = InvalidOperation.CanOnlyUseOnCreatures;
            return false;
        }

        if (caster.CurrentTarget != null && Range > 0 && caster.Location.GetMaxSqmDistance(caster.CurrentTarget.Location) > Range)
        {
            error = InvalidOperation.CreatureIsNotReachable;
            return false;
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

    public override string Words { get; set; }
    public static T Instance => Lazy.Value;
}