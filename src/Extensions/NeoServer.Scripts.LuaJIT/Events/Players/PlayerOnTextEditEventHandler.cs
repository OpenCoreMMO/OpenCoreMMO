using NeoServer.Domain.Common;
using NeoServer.Domain.Creatures.Events.Player;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Players;

public class PlayerOnTextEditEventHandler(ICreatureEvents creatureEvents) : IApplicationEventHandler<PlayerWroteTextEvent>
{
    public void Handle(PlayerWroteTextEvent @event)
    {
        if (@event is null) return;

        foreach (var creatureEvent in creatureEvents.GetCreatureEvents(@event.Player.CreatureId,
                     Enums.CreatureEventType.CREATURE_EVENT_TEXTEDIT))
            creatureEvent.ExecuteOnTextEdit(@event.Player, @event.Readable, @event.Text);
    }
}