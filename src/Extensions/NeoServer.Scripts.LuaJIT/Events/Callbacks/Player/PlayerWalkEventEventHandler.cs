using NeoServer.Domain.Common;
using NeoServer.Domain.Creatures.Events.Player;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Callbacks.Player;

public class PlayerWalkEventEventHandler(IEventsCallbacks eventsCallbacks)
    : IApplicationEventHandler<PlayerWalkEvent>
{
    public void Handle(PlayerWalkEvent @event)
    {
        eventsCallbacks.ExecuteCallback(
            EventCallbackType.PlayerOnWalk,
            callback =>
                callback.PlayerOnWalk(@event.Player, (byte)@event.Direction));
    }
}