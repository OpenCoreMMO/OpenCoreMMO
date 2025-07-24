using NeoServer.Domain.Common;
using NeoServer.Domain.Creatures.Events.Player;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Callbacks.Player;

public class PlayerRotateItemEventEventHandler(IEventsCallbacks eventsCallbacks)
    : IApplicationEventHandler<PlayerRotateItemEvent>
{
    public void Handle(PlayerRotateItemEvent @event)
    {
        eventsCallbacks.ExecuteCallback(
                EventCallbackType.PlayerOnRotateItem,
                callback =>
                    callback.PlayerOnRotateItem(@event.Player, @event.Item, @event.Position));
    }
}