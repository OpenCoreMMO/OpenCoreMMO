using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;
using NeoServer.Server.Common.Contracts.Scripts.Services;
using Serilog;

namespace NeoServer.Scripts.LuaJIT.Services;

public class LuaCreatureEventsScripts : ICreatureEventsScripts
{
    #region Members

    #endregion

    #region Dependency Injections

    /// <summary>
    /// A reference to the <see cref="ILogger"/> instance in use.
    /// </summary>
    private readonly ILogger _logger;

    /// <summary>
    /// A reference to the <see cref="ICreatureEvents"/> instance in use.
    /// </summary>
    private readonly ICreatureEvents _creatureEvents;

    #endregion

    #region Constructors

    public LuaCreatureEventsScripts(
        ILuaStartup luaStartup,
        ILogger logger,
        IActions actions,
        ICreatureEvents creatureEvents,
        IGlobalEvents globalEvents,
        ITalkActions talkActions)
    {
        _logger = logger;

        _creatureEvents = creatureEvents;
    }

    #endregion

    #region Public Methods 

    public void PlayerExtendedOpcodeHandle(IPlayer player, byte opcode, string buffer)
    {
        foreach (var creatureEvent in _creatureEvents.GetCreatureEvents(player.CreatureId, CreatureEventType.CREATURE_EVENT_EXTENDED_OPCODE))
            creatureEvent.ExecuteOnExtendedOpcode(player, opcode, buffer);
    }

    //public void CreatureEventExecuteOnPlayerLogin(IPlayer player) => _creatureEvents.PlayerLogin(player);

    //public void CreatureEventExecuteOnPlayerLogout(IPlayer player) => _creatureEvents.PlayerLogout(player);

    //public void CreatureEventExecuteOnThink(ICreature creature, int interval)
    //{
    //    foreach (var onThinkEvent in _creatureEvents.GetCreatureEvents(CreatureEventType.CREATURE_EVENT_THINK))
    //        onThinkEvent.ExecuteOnThink(creature, interval);
    //}

    #endregion
}
