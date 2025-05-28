using System;
using System.Linq;
using NeoServer.Game.Combat.Conditions;
using NeoServer.Game.Common;
using NeoServer.Game.Common.Combat.Enums;
using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.Spells;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Creatures.Players;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Results;
using NeoServer.Game.Common.Spell;

namespace NeoServer.Game.Combat.Spells;

public abstract class BaseSpell : ISpell
{
    public abstract uint Duration { get; }
    public virtual bool NeedsTarget { get; set; }

    public virtual byte? Range { get; set; }

    public abstract ConditionType ConditionType { get; }
    public virtual string Name { get; set; }
    public abstract EffectT Effect { get; }
    public virtual ushort Soul { get; set; }
    public virtual bool NeedsPremium { get; }
    public virtual ushort MinLevel { get; set; } = 0;
    public ushort MinMagicLevel { get; }
    public virtual bool NeedWeapon { get; }
    public virtual bool NeedLearn { get; }
    public virtual bool BlockWalls { get; set; }
    public virtual ushort ManaConsumption { get; set; }
    public ushort SoulConsumption { get; set; }
    public virtual string[] Vocations { get; }
    public virtual MagicGroup[] Groups { get; }
    public virtual uint[] GroupCooldown { get; }
    public virtual byte[] VocationIds { get; set; }
    public Guid CooldownId { get; } = Guid.NewGuid();
    public (int Id, uint Cooldown) PrimaryGroup { get; }
    public (int Id, uint Cooldown) SecondaryGroup { get; }
    public virtual uint Cooldown { get; set; }
    public bool IsAggressive { get; }
    public bool BlockingCreature { get; set; }
    public bool BlockingSolid { get; set; }
    public virtual bool NeedDirection { get; }
    public virtual bool CasterNeedsTargetOrDirection { get; }

    public Result Invoke(ICombatActor actor, IThing target, bool isHotkey)
    {
        if (!actor.HasCondition(ConditionType))
        {
            var castResult = OnCast(actor, target, isHotkey);
            if (castResult.Failed)
            {
                return castResult;
            }
        }

        AddCondition(actor);

        if (actor is IPlayer) AddCooldown(actor);

        OnSpellInvoked?.Invoke(actor, this);
        return Result.Success;
    }

    public virtual bool ShouldSay { get; }
    public abstract string Words { get; set; }
    public virtual bool Enabled => true;

    public static event InvokeSpell OnSpellInvoked;

    public abstract Result OnCast(ICombatActor caster, IThing target, bool isHotkey);

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

        var condition = new Condition(ConditionType, Duration)
        {
            EndAction = () => OnEnd(actor)
        };

        actor.AddCondition(condition);
    }

    private void AddCooldown(ICombatActor actor)
    {
        actor.StartSpellCooldown(this);
    }

    public Result CanCast(ICombatActor caster, IThing target)
    {
        if (caster is IPlayer player)
        {
            var validationResult = player.CanCastSpell(this);
            if (validationResult.Failed)
            {
                return validationResult;
            }
        }

        if (target is null && NeedsTarget)
        {
            return Result.Fail(InvalidOperation.CanOnlyUseOnCreatures);
        }

        if (caster.CurrentTarget != null && Range > 0 &&
            caster.Location.GetMaxSqmDistance(caster.CurrentTarget.Location) > Range)
        {
            return Result.Fail(InvalidOperation.CreatureIsNotReachable);
        }

        if (caster.Location.Z > target.Location.Z)
        {
            return Result.Fail(InvalidOperation.FirstGoUpStairs);
        }

        if (caster.Location.Z < target.Location.Z)
        {
            return Result.Fail(InvalidOperation.FirstGoDownStairs);
        }

        var targetCreature = target switch
        {
            ICreature creature => creature,
            IDynamicTile tile => tile.GetTopVisibleCreature(caster),
            _ => null
        };

        if (BlockingCreature && targetCreature is not null)
        {
            return Result.Fail(InvalidOperation.NotEnoughRoom);
        }

        if (target is IDynamicTile targetTile)
        {
            if (targetTile.HasFlag(TileFlags.BlockProjecTile) || targetTile.HasFlag(TileFlags.FloorChange) ||
                targetTile.HasTeleport(out _))
            {
                return Result.Fail(InvalidOperation.NotEnoughRoom);
            }

            if (IsAggressive && targetTile.HasFlag(TileFlags.ProtectionZone))
            {
                return Result.Fail(InvalidOperation.NotPermittedInProtectionZone);
            }

            if (BlockingSolid && targetTile.HasFlag(TileFlags.Unpassable))
            {
                return Result.Fail(InvalidOperation.NotEnoughRoom);
            }
        }

        if (NeedsTarget && targetCreature is null)
        {
            return Result.Fail(InvalidOperation.CanOnlyUseOnCreatures);
        }

        if (IsAggressive && NeedsTarget && targetCreature is not null && caster is IPlayer
            {
                SecureMode: PvpSecureMode.PvPDisabled
            } playerCaster)
        {
            if (targetCreature is IPlayer targetPlayer && !Equals(targetCreature, playerCaster) &&
                playerCaster.GetSkull(targetPlayer) == Skull.None &&
                !(playerCaster.Tile.PvpZone && targetPlayer.Tile.PvpZone))
            {
                return Result.Fail(InvalidOperation.TurnSecureModeToAttackUnmarkedPlayers);
            }
        }

        return Result.Success;
    }
}

public abstract class Spell<T> : BaseSpell where T : ISpell
{
    private static readonly Lazy<T> Lazy = new(() => (T)Activator.CreateInstance(typeof(T), true));
    public override bool ShouldSay => true;

    public override string Words { get; set; }
    public static T Instance => Lazy.Value;
}