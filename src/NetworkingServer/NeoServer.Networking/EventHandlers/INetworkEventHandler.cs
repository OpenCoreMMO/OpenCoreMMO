namespace NeoServer.Networking.EventHandlers;

public interface INetworkEventHandler<in T>
{
    void Subscribe(T entity);
    void Unsubscribe(T entity);
}