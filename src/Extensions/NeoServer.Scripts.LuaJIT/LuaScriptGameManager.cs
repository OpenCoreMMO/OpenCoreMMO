using NeoServer.Game.Common.Chats;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Game.Common.Contracts.World.Tiles;
using NeoServer.Game.Common.Location.Structs;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts.Scripts;
using Serilog;

namespace NeoServer.Scripts.LuaJIT;

public class LuaScriptGameManager : IScriptGameManager
{
    #region Members

    #endregion

    #region Dependency Injections

    /// <summary>
    /// A reference to the <see cref="ILuaStartup"/> instance in use.
    /// </summary>
    private readonly ILuaStartup _luaStartup;

    /// <summary>
    /// A reference to the <see cref="ILogger"/> instance in use.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// A reference to the <see cref="IActions"/> instance in use.
    /// </summary>
    private readonly IActions _actions;

    /// <summary>
    /// A reference to the <see cref="ICreatureEvents"/> instance in use.
    /// </summary>
    private readonly ICreatureEvents _creatureEvents;

    /// <summary>
    /// A reference to the <see cref="IGlobalEvents"/> instance in use.
    /// </summary>
    private readonly IGlobalEvents _globalEvents;

    /// <summary>
    /// A reference to the <see cref="ITalkActions"/> instance in use.
    /// </summary>
    private readonly ITalkActions _talkActions;

    /// <summary>
    /// A reference to the <see cref="IGlobalEvents"/> instance in use.
    /// </summary>
    private readonly IGlobalEvents _globalEvents;

    #endregion

    #region Constructors

    public LuaScriptGameManager(
        ILuaStartup luaStartup,
        ILogger logger,
        IActions actions,
        ICreatureEvents creatureEvents,
        IGlobalEvents globalEvents,
        ITalkActions talkActions)
    {
        _luaStartup = luaStartup;
        _logger = logger;

        _actions = actions;
        _creatureEvents = creatureEvents;
        _globalEvents = globalEvents;
        _talkActions = talkActions;
        _globalEvents = globalEvents;
    }

    #endregion

    #region Public Methods 

    public void Start()
    {
        _luaStartup.Start();
        _globalEvents.Startup();
    }

    //public bool HasTalkAction(string text) => _talkActions.TryGetTalkAction(text, out var talkactionWords, out var talkAction);

    public bool PlayerSaySpell(IPlayer player, SpeechType type, string words)
    {
        //if (!_talkActions.TryGetTalkAction(words, out var talkactionWords, out var talkAction))
        //    return false;

        var wordsSeparator = " ";
        var talkactionWords = words.Contains(wordsSeparator) ? words.Split(" ") : [words];

        if (!talkactionWords.Any())
            return false;

        var talkAction = _talkActions.GetTalkAction(talkactionWords[0]);

        if (talkAction == null)
            return false;

        var parameter = "";

        if (talkactionWords.Count() > 1)
            parameter = talkactionWords[1];

        return talkAction.ExecuteSay(player, talkactionWords[0], parameter, type);
    }

    public bool HasAction(IItem item) => _actions.GetAction(item) != null;

    public bool PlayerUseItem(IPlayer player, Location pos, byte stackpos, byte index, IItem item, IThing? target = null)
    {
        return PlayerUseItem(player, pos, pos, stackpos, item, target, false);
    }

    public bool PlayerUseItem(IPlayer player, Location fromPos, Location toPos, byte toStackPos, IItem item, IThing? target = null, bool isHotkey = false)
    {
        var action = _actions.GetAction(item);

        if (target != null)
        {
            if (target is ITile tile)
                target = tile.TopItemOnStack;
            else if (target is ICreature creature)
            {
                toPos = creature.Location;
                toStackPos = creature.Tile.GetCreatureStackPositionIndex(player);
            }
        }
        
        if (action != null)
            return action.ExecuteUse(
                player,
                item,
                player.Location,
                target,
                toPos,
                isHotkey);
        else
            _logger.Warning("Action with item id {ItemServerId} has not found into LuaJIT Scripts.", item.ServerId);

        return false;
    }

    public void GlobalEventExecuteRecord(int current, int old)
    {
        foreach (var (key, globalEvent) in _globalEvents.GetEventMap(GlobalEventType.GLOBALEVENT_RECORD))
            globalEvent.ExecuteRecord(current, old);
    }

    public void GlobalEventExecuteShutdown()
        => _globalEvents.Shutdown();

    public void GlobalEventExecuteSave()
        => _globalEvents.Save();

    public void CreatureEventExecuteOnPlayerLogin(IPlayer player) => _creatureEvents.PlayerLogin(player);
    
    public void CreatureEventExecuteOnPlayerLogout(IPlayer player) => _creatureEvents.PlayerLogout(player);

    #endregion
}
