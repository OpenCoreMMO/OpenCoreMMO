using NeoServer.Game.Common.Contracts.World;

namespace NeoServer.Game.Common.Contracts.Creatures;

public interface IMapEventSubscriber
{
    public void Subscribe(IMap map);
    public void Unsubscribe(IMap map);
}