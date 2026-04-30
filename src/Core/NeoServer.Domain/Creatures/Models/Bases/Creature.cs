using System.Diagnostics.CodeAnalysis;
using System.Net;
using NeoServer.Domain.Chat;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Common.Location.Structs;
using NeoServer.Domain.Creatures.Events;
using NeoServer.Domain.Creatures.Monster.Summon;
using NeoServer.Domain.Creatures.Player.Outfit;

namespace NeoServer.Domain.Creatures.Models.Bases;

public abstract class Creature : IEquatable<Creature>, ICreature
{
    protected readonly ICreatureType CreatureType;

    private IDynamicTile tile;

    public Creature(ICreatureType type, Outfit outfit = null, uint healthPoints = 0)
    {
        if (string.IsNullOrWhiteSpace(type.Name)) throw new ArgumentNullException(nameof(type.Name));
        MaxHealthPoints = type.MaxHealth;
        HealthPoints = type.Health > 0
            ? Math.Min(type.Health, MaxHealthPoints)
            : Math.Min(MaxHealthPoints, healthPoints == 0 ? MaxHealthPoints : healthPoints);

        CreatureType = type;

        CreatureId = RandomCreatureIdGenerator.Generate(this);

        Outfit = outfit ?? BuildOutfit(type);
    }

    public Action<ICreature> NextAction { get; protected set; }

    protected virtual string InspectionText => $"{Name}.";
    protected virtual string CloseInspectionText => $"{Name}.";
    public Direction LastDirection { get; protected set; }

    public IDynamicTile Tile
    {
        get => tile;
        protected set
        {
            tile = value;
            Location = tile.Location;
        }
    }

    public uint HealthPoints { get; set; }
    public uint MaxHealthPoints { get; set; }
    public string Name => CreatureType.Name;

    public string GetLookText(bool isClose = false, bool showInternalDetails = false)
    {
        return $"You see {(isClose ? CloseInspectionText : InspectionText)}";
    }

    public uint CreatureId { get; }
    public ushort CorpseType => CreatureType.Look[LookType.Corpse];
    public IThing Corpse { get; set; }
    public virtual BloodType BloodType => BloodType.Blood;
    public abstract Outfit Outfit { get; protected set; }
    public Outfit OriginalOutfit { get; set; }
    public Outfit LastOutfit { get; private set; }
    public Direction Direction { get; protected set; }
    public IList<Summon> Summons { get; protected set; } = new List<Summon>();

    public Direction SafeDirection
    {
        get
        {
            switch (Direction)
            {
                case Direction.North:
                case Direction.East:
                case Direction.South:
                case Direction.West:
                    return Direction;
                case Direction.NorthEast:
                case Direction.SouthEast:
                    return Direction.East;
                case Direction.NorthWest:
                case Direction.SouthWest:
                    return Direction.West;
                default:
                    return LastDirection;
            }
        }
    }

    public virtual void ChangeOutfit(Outfit outfit)
    {
        LastOutfit = null;
        Outfit.Change(outfit.LookType, outfit.Head, outfit.Body, outfit.Legs, outfit.Feet, outfit.Addon);
        OriginalOutfit = Outfit.Clone();

        EventAggregator.Invoke(new CreatureChangedOutfitEvent(this, Outfit));
    }

    public void SetTemporaryOutfit(ushort lookType, byte head, byte body, byte legs, byte feet,
        byte addon)
    {
        LastOutfit = Outfit.Clone();
        Outfit.Change(lookType, head, body, legs, feet, addon);
        EventAggregator.Invoke(new CreatureChangedOutfitEvent(this, Outfit));
    }

    public virtual void OnSpectatorMoved(ICreature spectator)
    {
    }

    public virtual void OnSpectatorDies(ICombatActor spectator)
    {
    }

    public virtual void OnSummonDie(Summon summon)
    {
    }

    public virtual void OnSpectatorLoggedOut(ICreature spectator)
    {
    }

    public virtual void OnSpectatorChangedVisibility(ICreature spectator)
    {
    }

    public virtual void OnMoving(ITile toTile)
    {
    }

    public void BackToOldOutfit()
    {
        Outfit = LastOutfit;
        LastOutfit = null;
        EventAggregator.Invoke(new CreatureChangedOutfitEvent(this, Outfit));
    }

    public byte LightLevel { get; protected set; }
    public byte LightColor { get; protected set; }
    public bool IsInvisible { get; protected set; } // TODO: implement.
    public abstract bool CanSeeInvisible { get; }

    public virtual bool CanSee(ICreature otherCreature)
    {
        if (otherCreature is null) return false;
        return !otherCreature.IsInvisible || CanSeeInvisible;
    }

    public virtual bool CanSee(Location pos)
    {
        return CanSee(pos, (int)MapViewPort.MaxViewPortX, (int)MapViewPort.MaxViewPortY);
    }

    public virtual bool IsThinking()
    {
        return true;
    }

    public virtual byte Emblem { get; } // TODO: implement.
    public bool IsHealthHidden { get; set; }
    public Location Location { get; private set; }

    public void SetNewLocation(Location location, bool force = false)
    {
        Location = location;
    }

    public void Say(string message, SpeechType talkType, ICreature receiver)
    {
        if (string.IsNullOrWhiteSpace(message) || talkType == SpeechType.None || receiver == null) return;

        if (receiver is not ISociableCreature sociableCreature) return;

        sociableCreature.Hear(this, talkType, message);
        EventAggregator.Invoke(new CreatureSayEvent(this, talkType, message,
            [sociableCreature]));
    }

    public void Say(string message, SpeechType talkType, List<ICreature> receivers)
    {
        if (string.IsNullOrWhiteSpace(message) || talkType == SpeechType.None || receivers == null ||
            receivers.Count == 0) return;

        foreach (var receiver in receivers)
        {
            if (receiver is ISociableCreature sociableCreature)
            {
                sociableCreature.Hear(this, talkType, message);
            }
        }

        EventAggregator.Invoke(new CreatureSayEvent(this, talkType, message, receivers));
    }

    public virtual void Think(int interval)
    {
        EventAggregator.Invoke(new CreatureThinkEvent(this, interval));
    }

    public virtual void Appear(Location location, ICylinderSpectator[] spectators)
    {
        foreach (var cylinderSpectator in spectators)
            cylinderSpectator.Spectator.OnCreatureAppear(this);
    }

    public void OnCreatureAppear(ICreature creature)
    {
        EventAggregator.Invoke(new CreatureAppearEvent(this, creature));
    }

    public virtual void OnCreatureDisappear(ICreature creature)
    {
        EventAggregator.Invoke(new CreatureDisappearEvent(this, creature));
    }

    public void OnMove(IWalkableCreature creature, IDynamicTile fromTile, IDynamicTile toTile)
    {
        EventAggregator.Invoke(new CreatureMoveEvent(this, creature, fromTile.Location, toTile.Location));
    }

    public void OnMoved(IThing to)
    {
    }

    public virtual void Use(IPlayer usedBy)
    {
    }

    public void SetLight(byte color, byte level)
    {
        LightColor = color;
        LightLevel = level;

        EventAggregator.Invoke(new CreatureChangedLightEvent(this));
    }

    public void RemoveLight()
    {
        SetLight(0, 0);
    }

    public virtual bool CanSee(Location pos, int viewRangeX, int viewRangeY, int limitRangeOffset = 0)
    {
        if (Location.IsSurface || Location.IsAboveSurface)
        {
            if (pos.IsUnderground) return false;
        }
        else if (Location.IsUnderground)
        {
            if (Math.Abs(Location.Z - pos.Z) > 2) return false;
        }

        var offsetZ = Location.Z - pos.Z;

        return pos.X >= Location.X - viewRangeX + offsetZ &&
               pos.X <= Location.X + viewRangeX + limitRangeOffset + offsetZ &&
               pos.Y >= Location.Y - viewRangeY + offsetZ &&
               pos.Y <= Location.Y + viewRangeY + limitRangeOffset + offsetZ;
    }

    public bool Equals([AllowNull] Creature other)
    {
        return this == other;
    }

    public virtual void Yell(string message, List<ICreature> listenersToYell) => Say(message, SpeechType.Yell, listenersToYell);

    public virtual void Whisper(string message, List<ICreature> listenersToWhisper) => Say(message, SpeechType.Whisper, listenersToWhisper);

    private Outfit BuildOutfit(ICreatureType type)
    {
        if (type?.Look is null) return new Outfit();

        return new Outfit
        {
            Addon = type.Look.TryGetValue(LookType.Addon, out var addon) ? (byte)addon : default,
            LookType = type.Look.TryGetValue(LookType.Type, out var lookType) ? lookType : default,
            Body = type.Look.TryGetValue(LookType.Body, out var body) ? (byte)body : default,
            Feet = type.Look.TryGetValue(LookType.Feet, out var feet) ? (byte)feet : default,
            Head = type.Look.TryGetValue(LookType.Head, out var head) ? (byte)head : default,
            Legs = type.Look.TryGetValue(LookType.Legs, out var legs) ? (byte)legs : default
        };
    }

    protected void ExecuteNextAction(ICreature creature)
    {
        NextAction?.Invoke(creature);
        NextAction = null;
    }

    protected void SetDirection(Direction direction)
    {
        if (direction == Direction.None) return;
        // LastDirection should only remember actual directions, so we're ignoring 'none'.
        LastDirection = Direction;
        Direction = direction;
    }

    public override bool Equals(object obj)
    {
        return obj is ICreature creature && creature.CreatureId == CreatureId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(CreatureId);
    }

    public static bool operator ==(Creature creature1, Creature creature2)
    {
        return creature1.CreatureId == creature2.CreatureId;
    }

    public static bool operator !=(Creature creature1, Creature creature2)
    {
        return creature1.CreatureId != creature2.CreatureId;
    }
}