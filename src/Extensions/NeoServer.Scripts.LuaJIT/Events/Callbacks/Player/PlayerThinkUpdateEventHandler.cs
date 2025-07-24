using NeoServer.Domain.Common;
using NeoServer.Domain.Creatures.Events.Player;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Callbacks.Player;

public class PlayerThinkUpdateEventHandler(IEventsCallbacks eventsCallbacks)
    : IApplicationEventHandler<PlayerThinkEvent>
{
    public void Handle(PlayerThinkEvent @event)
    {
        eventsCallbacks.ExecuteCallback(
                EventCallbackType.PlayerOnThink,
                callback =>
                    callback.PlayerOnThink(@event.Player, @event.Interval));
    }
}