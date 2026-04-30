using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Contracts.Items;
using NeoServer.Domain.Common.Contracts.World;
using NeoServer.Domain.Common.Contracts.World.Tiles;
using NeoServer.Domain.Common.Location;
using NeoServer.Domain.Creatures;
using NeoServer.Domain.Creatures.Monster.Summon;
using NeoServer.Domain.Creatures.Player.Outfit;

namespace NeoServer.Domain.Common.Contracts.Creatures;

public delegate void CreatureStateChange();

public delegate void RemoveCreature(ICreature creature);

public delegate void AddCondition(ICreature creature, ICondition condition);

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
    byte LightLevel { get; }

    /// <summary>
    ///     Creature's light color
    /// </summary>
    byte LightColor { get; }

    /// <summary>
    ///     Creature's outfit
    /// </summary>
    Outfit Outfit { get; }

    /// <summary>
    ///     Creature's Emblem
    /// </summary>
    byte Emblem { get; }

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
    Outfit LastOutfit { get; }

    /// <summary>
    ///     Tile which creature is on
    /// </summary>
    IDynamicTile Tile { get; }

    /// <summary>
    ///     Summons of creature
    /// </summary>
    IList<Summon> Summons { get; }

    Outfit OriginalOutfit { get; set; }

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
    void ChangeOutfit(Outfit outfit);

    /// <summary>
    ///     Set old outfit to current
    /// </summary>
    void BackToOldOutfit();

    void Appear(Location.Structs.Location location, ICylinderSpectator[] spectators);

    /// <summary>
    ///     Says a message
    /// </summary>
    //void Say(string message, SpeechType talkType, ICreature receiver = null);

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

    void SetLight(byte color, byte level);
    void RemoveLight();

    /// <summary>
    ///     Event that is fired when a spectator moves.
    /// </summary>
    /// <param name="spectator"></param>
    void OnSpectatorMoved(ICreature spectator);

    /// <summary>
    ///     Event that is fired when a spectator dies
    /// </summary>
    /// <param name="spectator"></param>
    void OnSpectatorDies(ICombatActor spectator);


    void OnSummonDie(Summon summon);
    void OnSpectatorLoggedOut(ICreature spectator);
    void OnSpectatorChangedVisibility(ICreature spectator);
    void OnMoving(ITile toTile);
    bool CanSee(Location.Structs.Location pos, int viewRangeX, int viewRangeY, int limitRangeOffset = 0);
    void Say(string message, SpeechType talkType, ICreature receiver);
    void Say(string message, SpeechType talkType, List<ICreature> receivers);
}