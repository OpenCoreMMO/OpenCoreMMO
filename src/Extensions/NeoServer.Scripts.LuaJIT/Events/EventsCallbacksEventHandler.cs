using NeoServer.Domain.Common;
using NeoServer.Domain.Creatures.Events.Player;
using NeoServer.Scripts.LuaJIT.Enums;
using NeoServer.Scripts.LuaJIT.Interfaces;

namespace NeoServer.Scripts.LuaJIT.Events.Creatures;

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
