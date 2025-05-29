using System;
using System.Collections.Generic;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;

namespace NeoServer.Game.Creatures.Models.Bases;

public abstract class WalkableCreature : Creature, IWalkableCreature
{
    private readonly Queue<Direction> _walkingQueue = new();

    protected readonly IMapTool MapTool;
    private uint _lastStepCost = 1;

    protected WalkableCreature(ICreatureType type,
        IMapTool mapTool,
        IOutfit outfit = null,
        uint healthPoints = 0) : base(type, outfit, healthPoints)
    {
        MapTool = mapTool;
        Speed = type.Speed;
        OnCompleteWalking += ExecuteNextAction;
    }

    internal CooldownList Cooldowns { get; } = new();
    public bool HasFollowPath { get; private set; }
    public virtual FindPathParams PathSearchParams => new(!HasFollowPath, true, true, false, 12, 1, 1, false);

    public virtual ITileEnterRule TileEnterRule => PlayerEnterTileRule.Rule;
    public virtual ushort Speed { get; protected set; }
    public ICreature Following { get; private set; }
    public bool IsFollowing => Following is not null;
    public bool HasNextStep => _walkingQueue.Count > 0;

    public virtual void OnMoved(IDynamicTile fromTile, IDynamicTile toTile, ICylinderSpectator[] spectators)
    {
        _lastStepCost = 1;

        if (fromTile.Location.Z != toTile.Location.Z || fromTile.Location.IsDiagonalMovement(toTile.Location))
            _lastStepCost = 2;
        SetDirection(fromTile.Location.DirectionTo(toTile.Location));

        if (_walkingQueue.IsEmpty()) OnCompleteWalking?.Invoke(this);
        OnCreatureMoved?.Invoke(this, fromTile.Location, toTile.Location, spectators);

        foreach (var spectator in spectators)
            spectator.Spectator.OnMove(this, fromTile, toTile);
    }

    public void TurnTo(ICreature creature)
        => TurnTo(Location.DirectionTo(creature.Location));

    public void TurnTo(Direction direction)
    {
        if (direction == Direction) return;
        SetDirection(direction);
        OnTurnedToDirection?.Invoke(this, direction);
    }

    public int StepDelay
    {
        get
        {
            if (FirstStep)
                return 0;

            if (Speed == 0) return 0;
            return (int)(Tile.StepSpeed / (decimal)Speed * 1000 * _lastStepCost);
        }
    }

    public bool FirstStep { get; private set; }

    public void StopWalking()
    {
        if (!HasNextStep) return;

        _walkingQueue.Clear();
        OnStoppedWalking?.Invoke(this);
    }

    public void CancelWalk()
    {
        _walkingQueue.Clear();
        OnCancelledWalking?.Invoke(this);
    }

    public void StopFollowing()
    {
        if (!IsFollowing) return;

        Following = null;
        HasFollowPath = false;
        StopWalking();
    }

    public virtual void Follow(ICreature creature)
    {
        Follow(creature, PathSearchParams);
    }

    public void Follow(ICreature creature, FindPathParams fpp)
    {
        if (creature is ICombatActor { IsDead: true })
        {
            StopFollowing();
            return;
        }

        if (Speed == 0) return;
        if (creature is null) return;

        if (IsFollowing)
        {
            Following = creature;
            StartFollowing(creature);
            return;
        }

        Following = creature;
        StartFollowing(creature);
        OnStartedFollowing?.Invoke(this, creature, fpp);
    }

    public virtual bool WalkTo(params Direction[] directions)
    {
        return TryWalkTo(directions);
    }

    public virtual bool WalkTo(Location location)
    {
        StopWalking();

        var (founded, directions) = MapTool.PathFinder.Find(this, location, PathSearchParams, TileEnterRule);

        if (founded) return TryWalkTo(directions);
        return false;
    }

    public virtual bool WalkTo(Location location, Action<ICreature> callbackAction)
    {
        StopWalking();

        var result = MapTool.PathFinder.Find(this, location, PathSearchParams, TileEnterRule);

        if (!result.Founded) return false;

        NextAction = callbackAction;
        return TryWalkTo(result.Directions);
    }

    public virtual bool WalkRandomStep()
    {
        var direction = GetRandomStep();

        if (direction == Direction.None) return false;

        TryWalkTo(direction);

        return true;
    }

    public void SetCurrentTile(IDynamicTile tile)
    {
        Tile = tile;
    }

    public virtual Direction GetNextStep()
    {
        if (!TryGetNextStep(out var direction)) return Direction.None;

        return direction;
    }

    public virtual void TeleportTo(Location location)
    {
        OnTeleported?.Invoke(this, location);
    }

    public virtual void TeleportTo(ushort x, ushort y, byte z)
    {
        OnTeleported?.Invoke(this, new Location(x, y, z));
    }

    public byte[] GetRaw(IPlayer playerRequesting)
    {
        return CreatureRaw.Convert(playerRequesting, this);
    }

    public void IncreaseSpeed(ushort speed)
    {
        ChangeSpeedLevel(speed + Speed);
    }

    public void DecreaseSpeed(ushort speedBoost)
    {
        ChangeSpeedLevel(Math.Max(0, Speed - speedBoost));
    }

    protected bool WalkRandomStep(Location origin, int maxStepsFromOrigin = 1)
    {
        var direction = GetRandomStep(origin, maxStepsFromOrigin);

        if (direction == Direction.None) return false;

        TryWalkTo(direction);

        return true;
    }

    public virtual void OnWalkableCreatureDisappear(ICreature creature)
    {
        StopFollowing();
    }

    public void StartFollowing(ICreature creature)
    {
        if (!CanSee(creature.Location, 9, 9))
        {
            OnWalkableCreatureDisappear(creature);
            return;
        }

        var result = MapTool.PathFinder.Find(this, creature.Location, PathSearchParams, TileEnterRule);

        if (!result.Founded)
        {
            HasFollowPath = false;
            return;
        }

        HasFollowPath = true;
        TryUpdatePath(result.Directions);
    }

    public virtual bool TryWalkTo(params Direction[] directions)
    {
        if (Speed == 0) return false;

        if (!_walkingQueue.IsEmpty()) _walkingQueue.Clear();

        if (directions.Length >= 1 && Cooldowns.Expired(CooldownType.Move)) FirstStep = true;

        foreach (var direction in directions)
        {
            if (direction == Direction.None) continue;

            _walkingQueue.Enqueue(direction);
        }

        if (_walkingQueue.IsEmpty()) return true;

        OnStartedWalking?.Invoke(this);
        return true;
    }

    protected Direction GetRandomStep()
    {
        return MapTool.PathFinder.FindRandomStep(this, TileEnterRule);
    }

    private Direction GetRandomStep(Location origin, int maxStepsFromOrigin = 1)
    {
        return MapTool.PathFinder.FindRandomStep(this, TileEnterRule, origin, maxStepsFromOrigin);
    }

    public bool TryUpdatePath(Direction[] newPath)
    {
        if (newPath.Length == 0) return false;
        if (!Cooldowns.Expired(CooldownType.UpdatePath)) return false;

        Cooldowns.Start(CooldownType.UpdatePath, 1000);

        TryWalkTo(newPath);

        return true;
    }

    private bool TryGetNextStep(out Direction direction)
    {
        if (_walkingQueue.TryDequeue(out direction))
        {
            FirstStep = false;
            Cooldowns.Start(CooldownType.Move, StepDelay);

            return true;
        }

        return false;
    }

    public void ChangeSpeedLevel(int newSpeed)
    {
        Speed = (ushort)newSpeed;
        OnChangedSpeed?.Invoke(this, Speed);
    }

    #region Events

    public event StopWalk OnCompleteWalking;
    public event StartWalk OnStartedWalking;
    public event TurnedToDirection OnTurnedToDirection;
    public event StartFollow OnStartedFollowing;
    public event ChangeSpeed OnChangedSpeed;
    public event TeleportTo OnTeleported;
    public event Moved OnCreatureMoved;
    public event StopWalk OnStoppedWalking;
    public event StopWalk OnCancelledWalking;

    #endregion
}