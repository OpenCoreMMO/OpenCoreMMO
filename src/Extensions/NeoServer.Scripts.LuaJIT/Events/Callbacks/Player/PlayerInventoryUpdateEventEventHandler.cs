using NeoServer.Domain.Common;
using NeoServer.Domain.Creatures.Events.Player;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Callbacks.Player;

public class PlayerInventoryUpdateEventEventHandler(IEventsCallbacks eventsCallbacks)
    : IApplicationEventHandler<PlayerInventoryUpdateEvent>
{
    public void Handle(PlayerInventoryUpdateEvent @event)
    {
        eventsCallbacks.ExecuteCallback(
                EventCallbackType.PlayerOnInventoryUpdate,
                callback =>
                    callback.PlayerOnInventoryUpdate(@event.Player, @event.Item, @event.Slot, @event.Equip));
    }
}