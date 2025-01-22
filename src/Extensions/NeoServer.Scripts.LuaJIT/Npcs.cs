using NeoServer.Scripts.LuaJIT.Enums;
using Serilog;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Game.Common.Helpers;

namespace NeoServer.Scripts.LuaJIT;

public class NpcEvents
{
    public LuaScriptInterface LuaScriptInterface { get; set; }
    public Dictionary<NpcsEventType, int?> Events { get; set; }

    public NpcEvents()
    {
        Events = new Dictionary<NpcsEventType, int?>();
    }
}

public class Npcs : INpcs
{
    private readonly ILogger _logger;

    #region Constructors

    public Npcs(
        ILogger logger)
    {
        _logger = logger;
    }

    #endregion

    #region Members

    private readonly Dictionary<string, NpcEvents> _npcEventsMap = new();

    #endregion

    #region Public Methods

    public void Add(string npcName, NpcsEventType eventType, LuaScriptInterface luaScriptInterface)
    {
        if (!_npcEventsMap.TryGetValue(npcName, out var npcEvents))
            npcEvents = new NpcEvents { LuaScriptInterface = luaScriptInterface };
        
        npcEvents.Events.Add(eventType, null);
        _npcEventsMap.AddOrUpdate(npcName, npcEvents);
    }

    public bool LoadCallback(string npcName)
    {
        if (!_npcEventsMap.TryGetValue(npcName, out var npcEvents))
            return false;

        var id = npcEvents.LuaScriptInterface.GetEvent();
        if (id == -1)
        {
            _logger.Warning("[NpcType::loadCallback] - Event not found");
            return false;
        }

        npcEvents.Events.AddOrUpdate(npcEvents.Events.Keys.LastOrDefault(), id);
        //npcEvents.AddOrUpdate(npcEvents.Events.LastOrDefault(), id);
        return true;
    }

    public NpcEvents GetEvents(string npcName)
    {
        _npcEventsMap.TryGetValue(npcName, out var npcEvents);
        return npcEvents;
    }

    public void Clear()
    {
        _npcEventsMap.Clear();
    }

    #endregion
}