using NeoServer.Game.Common.Contracts.World;
using NeoServer.Server.Common.Contracts.Tasks;
using NeoServer.Server.Common.Enums;

namespace NeoServer.Server.Common.Contracts;

public delegate void OpenServer();

public interface IGameServer
{
    IGameCreatureManager CreatureManager { get; }
    IMap Map { get; }
    IDispatcher Dispatcher { get; }
    IScheduler Scheduler { get; }
    IDecayableItemManager DecayableItemManager { get; }
    GameState State { get; }
    IPersistenceDispatcher PersistenceDispatcher { get; }
    void Close();
    void Open();
    event OpenServer OnOpened;
}