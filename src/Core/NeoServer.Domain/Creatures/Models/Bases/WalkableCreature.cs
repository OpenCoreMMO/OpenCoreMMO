using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Domain.Creatures.Player.Outfit;

namespace NeoServer.Domain.Creatures.Models.Bases;

public abstract class WalkableCreature : Creature, IWalkableCreature
{
    private readonly Queue<Direction> _walkingQueue = new();

    protected readonly IMapTool MapTool;
    private bool _forceUpdateFollowPath;
    private uint _lastStepCost = 1;
    private uint _walkUpdateTicks;

    protected WalkableCreature(ICreatureType type,
        IMapTool mapTool,
        Outfit outfit = null,
        uint healthPoints = 0) : base(type, outfit, healthPoints)
    {
        MapTool = mapTool;
        Speed = type.Speed;
        RawSpeed = type.Speed;
    }

    internal CooldownList Cooldowns { get; } = new();
    public bool HasFollowPath { get; private set; }
    public virtual FindPathParams PathSearchParams => new(!HasFollowPath, true, true, false, 12, 1, 1);
    public virtual ushort RawSpeed { get; protected set; }

    public virtual ITileEnterRule TileEnterRule => PlayerEnterTileRule.Rule;
    public virtual ushort Speed { get; protected set; }

    public ICreature FollowCreature { get; private set; }
    public bool IsFollowing => FollowCreature is not null;
    public bool HasNextStep => _walkingQueue.Count > 0;

    public virtual void OnMoved(IDynamicTile fromTile, IDynamicTile toTile, ICylinderSpectator[] spectators)
    {
        _lastStepCost = 1;

        if (fromTile.Location.Z != toTile.Location.Z || fromTile.Location.IsDiagonalMovement(toTile.Location))
            _lastStepCost = 2;
        SetDirection(fromTile.Location.DirectionTo(toTile.Location));

        if (_walkingQueue.IsEmpty())
        {
            EventAggregator.Invoke(new CreatureCompletedWalkingEvent(this));
            ExecuteNextAction(this);
        }
        
        OnCreatureMoved?.Invoke(this, fromTile.Location, toTile.Location, spectators);

        foreach (var spectator in spectators)
            spectator.Spectator.OnMove(this, fromTile, toTile);
    }

    public void TurnTo(ICreature creature)
    {
        if (creature is null) return;
        TurnTo(Location.DirectionTo(creature.Location));
    }

    public void TurnTo(Direction direction)
    {
        if (direction is Direction.None) return;
        if (direction == Direction) return;

        SetDirection(direction);
        EventAggregator.Invoke(new CreatureTurnedToDirectionEvent(this, direction));
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
        EventAggregator.Invoke(new CreatureStoppedWalkingEvent(this));
    }
    
    public void CancelWalk()
    {
        _walkingQueue.Clear();
        EventAggregator.Invoke(new CreatureCancelledWalkingEvent(this));
    }

    public void StopFollowing()
    {
        if (!IsFollowing) return;

        FollowCreature = null;
        HasFollowPath = false;
        _walkUpdateTicks = 0;
        _forceUpdateFollowPath = false;

        EventAggregator.Invoke(new StoppedFollowEvent(this));
        
        StopWalking();
    }

    public override void Think(int interval)
    {
        base.Think(interval);

        if (IsFollowing)
        {
            _walkUpdateTicks += (uint)interval;

            if (_forceUpdateFollowPath || _walkUpdateTicks >= 2000)
            {
                _walkUpdateTicks = 0;
                _forceUpdateFollowPath = false;
                Follow(FollowCreature);
            }
        }
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

        if (!CanSee(creature.Location))
        {
            StopFollowing();
            return;
        }

        if (IsFollowing)
        {
            FollowCreature = creature;
            StartFollowing(creature);
            return;
        }

        FollowCreature = creature;
        _forceUpdateFollowPath = false;

        StartFollowing(creature);
        EventAggregator.Invoke(new CreatureStartedFollowingEvent(this, creature, fpp));
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

        if (!result.Found) return false;

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
        if (speed == 0) return;
        ChangeSpeedLevel(speed + Speed);
    }

    public void DecreaseSpeed(ushort speedBoost)
    {
        ChangeSpeedLevel(Math.Max(0, Speed - speedBoost));
    }

    public override void OnSpectatorMoved(ICreature spectator)
    {
        if (Equals(spectator, FollowCreature)) //followed creature moved
        {
            // If we have no more steps in our walk queue, immediately recalculate the follow path
            if (!HasNextStep && HasFollowPath)
            {
                _forceUpdateFollowPath = false;
                Follow(FollowCreature);
            }
            else
            {
                _forceUpdateFollowPath = true;
            }
        }
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

        if (!result.Found)
        {
            HasFollowPath = false;
            StopWalking();
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

        EventAggregator.Invoke(new CreatureStartedWalkingEvent(this));
        return true;
    }

    protected virtual Direction GetRandomStep()
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

        TryWalkTo(newPath);

        return true;
    }

    private bool TryGetNextStep(out Direction direction)
    {
        if (_walkingQueue.TryDequeue(out direction))
        {
            FirstStep = false;
            Cooldowns.Start(CooldownType.Move, (uint)StepDelay);

            return true;
        }

        return false;
    }

    public void ChangeSpeedLevel(int newSpeed)
    {
        Speed = (ushort)newSpeed;
        EventAggregator.Invoke(new CreatureChangedSpeedEvent(this, Speed));
    }

    #region Events

    public event TeleportTo OnTeleported;
    public event Moved OnCreatureMoved;

    #endregion
}