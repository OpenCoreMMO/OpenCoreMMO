using NeoServer.Game.Common.Contracts;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Players;

public class PlayerOnTextEditEventHandler : IGameEventHandler
{
    private readonly ICreatureEvents _creatureEvents;

    public PlayerOnTextEditEventHandler(ICreatureEvents creatureEvents)
    {
        _creatureEvents = creatureEvents;
    }

    public void Execute(IPlayer player, IItem item, string text)
    {
        foreach (var creatureEvent in _creatureEvents.GetCreatureEvents(player.CreatureId, CreatureEventType.CREATURE_EVENT_TEXTEDIT))
            creatureEvent.ExecuteOnTextEdit(player, item, text);
    }
}