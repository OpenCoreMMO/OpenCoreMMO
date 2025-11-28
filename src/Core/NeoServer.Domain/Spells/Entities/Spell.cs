using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Combat.Enums;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.Spells;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Results;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Domain.Creatures.Conditions.Implementations;
using NeoServer.Domain.Creatures.Player.Modes;

namespace NeoServer.Domain.Spells.Entities;

public abstract class BaseSpell : ISpell
{
    public abstract uint Duration { get; }

    public abstract ConditionType ConditionType { get; }
    public virtual string[] Groups { get; }
    public virtual bool NeedsTarget { get; set; }

    public virtual byte? Range { get; set; }
    public virtual string Name { get; set; }
    public abstract EffectT Effect { get; }
    public virtual bool NeedsPremium { get; set; }
    public virtual ushort MinLevel { get; set; } = 0;
    public ushort MinMagicLevel { get; set; }
    public virtual bool NeedWeapon { get; set; }
    public virtual bool NeedLearn { get; set; }
    public virtual bool BlockWalls { get; set; }
    public virtual ushort ManaConsumption { get; set; }
    public virtual ushort ManaPercent { get; set; }
    public ushort SoulConsumption { get; set; }
    public virtual string[] Vocations { get; }
    public virtual byte[] VocationIds { get; set; }
    public Guid CooldownId { get; } = Guid.NewGuid();
    public (string Name, uint Cooldown) PrimaryGroup { get; set; }
    public (string Name, uint Cooldown) SecondaryGroup { get; set; }
    public virtual uint Cooldown { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsSelfTarget { get; set; }
    public bool IsAggressive { get; set; }
    public bool BlockingCreature { get; set; }
    public bool BlockingSolid { get; set; }
    public virtual bool NeedDirection { get; set; }
    public virtual bool NeedCasterTargetOrDirection { get; set; }
    public virtual bool HasParams { get; set; }
    public virtual object[] Params { get; set; }

    public Result Invoke(ICombatActor actor, IThing target, bool isHotkey)
    {
        if (!actor.HasCondition(ConditionType))
        {
            var castResult = OnCast(actor, target, isHotkey);
            if (castResult.Failed) return castResult;
        }

        AddCondition(actor);

        OnSpellInvoked?.Invoke(actor, this);
        return Result.Success;
    }

    public virtual bool ShouldSay { get; }
    public abstract string Words { get; set; }

    public Result CanCast(ICombatActor caster, IThing target)
    {
        if (caster is IPlayer player)
        {
            var validationResult = player.CanCastSpell(this);
            if (validationResult.Failed) return validationResult;
        }

        if (target is null && NeedsTarget) return Result.Fail(InvalidOperation.CanOnlyUseOnCreatures);

        if (caster.CurrentTarget != null && Range > 0 &&
            caster.Location.GetMaxSqmDistance(caster.CurrentTarget.Location) > Range)
            return Result.Fail(InvalidOperation.CreatureIsNotReachable);

        if (NeedsTarget && caster.Location.Z > target.Location.Z) return Result.Fail(InvalidOperation.FirstGoUpStairs);

        if (NeedsTarget && caster.Location.Z < target.Location.Z)
            return Result.Fail(InvalidOperation.FirstGoDownStairs);

        var targetCreature = target switch
        {
            ICreature creature => creature,
            IDynamicTile tile => tile.GetTopVisibleCreature(caster),
            _ => null
        };

        if (BlockingCreature && targetCreature is not null) return Result.Fail(InvalidOperation.NotEnoughRoom);

        if (target is IDynamicTile targetTile)
        {
            if (targetTile.HasFlag(TileFlags.BlockProjecTile) || targetTile.HasFlag(TileFlags.FloorChange) ||
                targetTile.HasTeleport(out _))
                return Result.Fail(InvalidOperation.NotEnoughRoom);

            if (IsAggressive && targetTile.HasFlag(TileFlags.ProtectionZone))
                return Result.Fail(InvalidOperation.NotPermittedInProtectionZone);

            if (BlockingSolid && targetTile.HasFlag(TileFlags.Unpassable))
                return Result.Fail(InvalidOperation.NotEnoughRoom);
        }

        if (NeedsTarget && targetCreature is null) return Result.Fail(InvalidOperation.CanOnlyUseOnCreatures);

        if (IsAggressive && NeedsTarget && targetCreature is not null && caster is IPlayer
            {
                SecureMode: PvpSecureMode.PvPDisabled
            } playerCaster)
            if (targetCreature is IPlayer targetPlayer && !Equals(targetCreature, playerCaster) &&
                playerCaster.GetSkull(targetPlayer) == Skull.None &&
                !(playerCaster.Tile.PvpZone && targetPlayer.Tile.PvpZone))
                return Result.Fail(InvalidOperation.TurnSecureModeToAttackUnmarkedPlayers);

        return Result.Success;
    }

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
}

public abstract class Spell<T> : BaseSpell where T : ISpell
{
    private static readonly Lazy<T> Lazy = new(() => (T)Activator.CreateInstance(typeof(T), true));
    public override bool ShouldSay => true;
    public override string Words { get; set; }
    public static T Instance => Lazy.Value;
}