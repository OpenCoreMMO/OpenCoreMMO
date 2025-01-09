using NeoServer.Game.Creatures.Events;

namespace NeoServer.Networking.EventHandlers;

public interface INetworkEventHandler<in T> 
{
    void Subscribe(T entity);
}