using System.Collections.Generic;
using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Creatures;
using NeoServer.Game.Common.Location;

namespace NeoServer.Game.Common.Contracts.Creatures;

public delegate void CreatureStateChange();

public delegate void TurnedToDirection(IWalkableCreature creature, Direction direction);

public delegate void RemoveCreature(ICreature creature);

public delegate void StopWalk(IWalkableCreature creature);

public delegate void BeforeDeath(ICombatActor creature, ICombatActor killer, int realDamage);

public delegate void Death(ICombatActor creature, IThing by, ILoot loot);

public delegate void Kill(ICombatActor creature, ICombatActor target, bool lastHit);

public delegate void GainExperience(ICreature creature, long exp);

public delegate void LoseExperience(ICreature creature, long exp);

public delegate void StartWalk(IWalkableCreature creature);

public delegate void Say(ICreature creature, SpeechType type, string message, ICreature receiver = null);

public delegate void AddCondition(ICreature creature, ICondition condition);

public delegate void RemoveCondition(ICreature creature, ICondition condition);

public delegate void ChangeOutfit(ICreature creature, IOutfit outfit);

public delegate void Think(ICreature creature, int interval);

public delegate void Appear(ICreature self, ICreature creature);
public delegate void Disappear(ICreature self, ICreature creature);

public delegate void CreatureMove(
    ICreature self,
    ICreature creature,
    Location.Structs.Location fromLocation,
    Location.Structs.Location toLocation);

public interface ICreature : IMovableThing
{
    /// <summary>
    ///     Creature's Blood Type. Ex: Slime, blood, fire ...
    /// </summary>
    BloodType BloodType { get; }

    /// <summary>
    ///     Checks if creature can see invisible creatures
    /// </summary>
    bool CanSeeInvisible { get; }

    /// <summary>
    ///     Translates directions to only South ,North, East or West direction;
    ///     NorthEast and SouthEast to: East;
    ///     NorthWest and SouthWest to West
    /// </summary>
    Direction SafeDirection { get; }

    /// <summary>
    ///     Corpse Type Id
    /// </summary>
    ushort CorpseType { get; }

    /// <summary>
    ///     Random Creature Id
    /// </summary>
    uint CreatureId { get; }

    /// <summary>
    ///     Player Direction: North, South, East and West
    /// </summary>
    Direction Direction { get; }

    /// <summary>
    ///     Checks if Creature is invisible
    /// </summary>
    bool IsInvisible { get; }

    /// <summary>
    ///     Creature's light level
    /// </summary>
    byte LightBrightness { get; }

    /// <summary>
    ///     Creature's light color
    /// </summary>
    byte LightColor { get; }

    /// <summary>
    ///     Creature's outfit
    /// </summary>
    IOutfit Outfit { get; }

    /// <summary>
    ///     Creature's Emblem
    /// </summary>
    byte Emblem { get; }

    /// <summary>
    ///     Indicates Skull showed on creature
    /// </summary>
    byte Skull { get; }

    /// <summary>
    ///     HP
    /// </summary>
    uint HealthPoints { get; set; }

    /// <summary>
    ///     Maximum HP
    /// </summary>
    uint MaxHealthPoints { get; set; }

    /// <summary>
    ///     Indicates if HP is displayed
    /// </summary>
    bool IsHealthHidden { get; set; }

    /// <summary>
    ///     Corpse instance
    /// </summary>
    IThing Corpse { get; set; }

    /// <summary>
    ///     Last outfit creature used
    /// </summary>
    IOutfit LastOutfit { get; }

    /// <summary>
    ///     Tile which creature is on
    /// </summary>
    IDynamicTile Tile { get; }

    /// <summary>
    ///     Checks if creature can be seen by others
    /// </summary>
    bool CanBeSeen { get; }
    
    /// <summary>
    ///     Summons of creature
    /// </summary>
    IList<IMonster> Summons { get; }

    /// <summary>
    ///     Fires when creature is removed from game
    /// </summary>
    event RemoveCreature OnCreatureRemoved;

    /// <summary>
    ///     Fires when creature says something
    /// </summary>
    event Say OnSay;

    /// <summary>
    ///     Fires when creature thinks something
    /// </summary>
    event Think OnThink;

    /// <summary>
    ///     Fires when creature appear
    /// </summary>
    event Appear OnAppear;

    /// <summary>
    ///     Fires when creature disappear
    /// </summary>
    event Disappear OnDisappear;

    /// <summary>
    ///     Fires when creature changes outfit
    /// </summary>
    event ChangeOutfit OnChangedOutfit;

    /// <summary>
    ///     Fires when creature move
    /// </summary>
    event CreatureMove OnCreatureMove;

    /// <summary>
    ///     Checks if creature can see other creature
    /// </summary>
    bool CanSee(ICreature otherCreature);

    /// <summary>
    ///     Checks if creature can see location
    /// </summary>
    /// <returns></returns>
    bool CanSee(Location.Structs.Location pos);
    
    /// <summary>
    ///     Checks if creature can execute think
    /// </summary>
    /// <returns></returns>
    bool IsThinking();

    /// <summary>
    ///     Change creature outfit
    /// </summary>
    void ChangeOutfit(IOutfit outfit);

    /// <summary>
    ///     Set old outfit to current
    /// </summary>
    void BackToOldOutfit();

    void Appear(Location.Structs.Location location, ICylinderSpectator[] spectators);

    void Disappear(Location.Structs.Location location, ICylinderSpectator[] spectators);

    /// <summary>
    ///     Says a message
    /// </summary>
    void Say(string message, SpeechType talkType, ICreature receiver = null);

    /// <summary>
    ///     Thinks something
    /// </summary>

    void Think(int interval);

    void OnCreatureAppear(ICreature creature);

    void OnCreatureDisappear(ICreature creature);

    void OnMove(IWalkableCreature creature, IDynamicTile fromTile, IDynamicTile toTile);

    /// <summary>
    ///     Sets new outfit and store current as last outfit
    /// </summary>
    void SetTemporaryOutfit(ushort lookType, byte head, byte body, byte legs, byte feet, byte addon);
}