using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Players;

public class PlayerOnExtendedOpcodeEventHandler : IGameEventHandler
{
    private readonly ICreatureEvents _creatureEvents;

    public PlayerOnExtendedOpcodeEventHandler(ICreatureEvents creatureEvents)
    {
        _creatureEvents = creatureEvents;
    }

    public void Execute(IPlayer player, byte opcode, string buffer)
    {
        foreach (var creatureEvent in _creatureEvents.GetCreatureEvents(player.CreatureId, CreatureEventType.CREATURE_EVENT_EXTENDED_OPCODE))
            creatureEvent.ExecuteOnExtendedOpcode(player, opcode, buffer);
    }
}