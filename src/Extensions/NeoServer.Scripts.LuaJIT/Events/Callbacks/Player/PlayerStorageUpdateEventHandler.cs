using NeoServer.Domain.Common;
using NeoServer.Domain.Creatures.Events.Player;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Callbacks.Player;

public class PlayerStorageUpdateEventHandler(IEventsCallbacks eventsCallbacks)
    : IApplicationEventHandler<PlayerStorageUpdateEvent>
{
    public void Handle(PlayerStorageUpdateEvent @event)
    {
        eventsCallbacks.ExecuteCallback(
                EventCallbackType.PlayerOnStorageUpdate,
                callback =>
                    callback.PlayerOnStorageUpdate(@event.Player, @event.Key, @event.Value, @event.OldValue, @event.CurrentTime));
    }
}