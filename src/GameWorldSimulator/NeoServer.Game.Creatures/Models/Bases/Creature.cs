using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Helpers;
using NeoServer.Game.Common.Location;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Game.Creatures.Player;

namespace NeoServer.Game.Creatures.Models.Bases;

public abstract class Creature : IEquatable<Creature>, ICreature
{
    protected readonly ICreatureType CreatureType;

    private IDynamicTile tile;

    public Creature(ICreatureType type, IOutfit outfit = null, uint healthPoints = 0)
    {
        if (string.IsNullOrWhiteSpace(type.Name)) throw new ArgumentNullException(nameof(type.Name));
        MaxHealthPoints = type.MaxHealth;
        HealthPoints = Math.Min(MaxHealthPoints, healthPoints == 0 ? MaxHealthPoints : healthPoints);

        CreatureType = type;

        CreatureId = RandomCreatureIdGenerator.Generate(this);

        Outfit = outfit ?? BuildOutfit(type);

        MaxHealthPoints = type.MaxHealth;
    }

    public Action<ICreature> NextAction { get; protected set; }

    protected virtual string InspectionText => $"{Name}.";
    protected virtual string CloseInspectionText => $"{Name}.";
    public Direction LastDirection { get; protected set; }

    public event RemoveCreature OnCreatureRemoved;

    public event ChangeOutfit OnChangedOutfit;

    public event Say OnSay;

    public event Think OnThink;

    public IDynamicTile Tile
    {
        get => tile;
        protected set
        {
            tile = value;
            Location = tile.Location;
        }
    }

    public uint HealthPoints { get; protected set; }
    public uint MaxHealthPoints { get; protected set; }
    public string Name => CreatureType.Name;

    public string GetLookText(bool isClose = false, bool showInternalDetails = false)
    {
        return $"You see {(isClose ? CloseInspectionText : InspectionText)}";
    }

    public uint CreatureId { get; }
    public ushort CorpseType => CreatureType.Look[LookType.Corpse];
    public IThing Corpse { get; set; }
    public virtual BloodType BloodType => BloodType.Blood;
    public abstract bool CanBeSeen { get; }
    public abstract IOutfit Outfit { get; protected set; }
    public IOutfit LastOutfit { get; private set; }
    public Direction Direction { get; protected set; }
    public IList<IMonster> Summons { get; protected set; } = new List<IMonster>();

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

    public virtual void ChangeOutfit(IOutfit outfit)
    {
        LastOutfit = null;
        Outfit.Change(outfit.LookType, outfit.Head, outfit.Body, outfit.Legs, outfit.Feet, outfit.Addon);

        OnChangedOutfit?.Invoke(this, Outfit);
    }

    public void SetTemporaryOutfit(ushort lookType, byte head, byte body, byte legs, byte feet,
        byte addon)
    {
        LastOutfit = Outfit.Clone();
        Outfit.Change(lookType, head, body, legs, feet, addon);
        OnChangedOutfit?.Invoke(this, Outfit);
    }

    public void BackToOldOutfit()
    {
        Outfit = LastOutfit;
        LastOutfit = null;
        OnChangedOutfit?.Invoke(this, Outfit);
    }

    public byte LightBrightness { get; protected set; }
    public byte LightColor { get; protected set; }
    public bool IsInvisible { get; protected set; } // TODO: implement.
    public abstract bool CanSeeInvisible { get; }

    public virtual bool CanSee(ICreature otherCreature)
    {
        return !otherCreature.IsInvisible || CanSeeInvisible;
    }

    public virtual void OnAppear(Location location, ICylinderSpectator[] spectators)
    {
    }

    public virtual bool CanSee(Location pos)
    {
        return CanSee(pos, (int)MapViewPort.MaxViewPortX, (int)MapViewPort.MaxViewPortY);
    }

    public byte Skull { get; protected set; } // TODO: implement.

    public virtual byte Emblem { get; } // TODO: implement.
    public bool IsHealthHidden { get; protected set; }
    public Location Location { get; private set; }

    public void SetNewLocation(Location location, bool force = false)
    {
        Location = location;
    }

    public virtual void Say(string message, SpeechType talkType, ICreature receiver = null)
    {
        if (string.IsNullOrWhiteSpace(message) || talkType == SpeechType.None) return;
        OnSay?.Invoke(this, talkType, message, receiver);
    }

    public virtual void Think(int interval)
    {
        OnThink?.Invoke(this, interval);
    }

    public void OnMoved(IThing to)
    {
    }

    public virtual void Use(IPlayer usedBy)
    {
    }

    public bool Equals([AllowNull] Creature other)
    {
        return this == other;
    }

    private IOutfit BuildOutfit(ICreatureType type)
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

    protected bool CanSee(Location pos, int viewRangeX, int viewRangeY, int limitRangeOffset = 0)
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

    protected void ExecuteNextAction(ICreature creature)
    {
        NextAction?.Invoke(creature);
        NextAction = null;
    }

    protected void SetDirection(Direction direction)
    {
        // LastDirection should only remember actual directions, so we're ignoring 'none'.
        LastDirection = Direction == Direction.None ? LastDirection : Direction;
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