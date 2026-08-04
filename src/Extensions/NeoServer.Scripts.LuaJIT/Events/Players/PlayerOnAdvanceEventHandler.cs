using NeoServer.Domain.Common;
using NeoServer.Domain.Creatures.Events.Player;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Players;

public class PlayerOnAdvanceEventHandler(ICreatureEvents creatureEvents) : IApplicationEventHandler<PlayerLevelAdvancedEvent>
{
    public void Handle(PlayerLevelAdvancedEvent @event)
    {
        if (@event is null) return;
        creatureEvents.PlayerAdvance(@event.Player, @event.Type, @event.FromLevel, @event.ToLevel);
    }
}