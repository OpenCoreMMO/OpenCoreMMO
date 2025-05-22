using System.Diagnostics.CodeAnalysis;
using NeoServer.Game.Common.Contracts.World;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Tasks;
using NeoServer.Server.Common.Enums;

namespace NeoServer.Server;

public class GameServer : IGameServer
{
    public GameServer(IMap map,
        IDispatcher dispatcher, IScheduler scheduler, IGameCreatureManager creatureManager,
        IDecayableItemManager decayableBag, IPersistenceDispatcher persistenceDispatcher)
    {
        Map = map;
        Dispatcher = dispatcher;
        Scheduler = scheduler;
        CreatureManager = creatureManager;
        DecayableItemManager = decayableBag;
        PersistenceDispatcher = persistenceDispatcher;
        Instance = this;
    }

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public static IGameServer Instance { get; private set; }

    /// <summary>
    ///     Game state
    /// </summary>
    /// <value></value>
    public GameState State { get; private set; }

    /// <summary>
    ///     Map instance
    /// </summary>
    /// <value></value>
    public IMap Map { get; }

    public IGameCreatureManager CreatureManager { get; }
    public IDecayableItemManager DecayableItemManager { get; }
    public IPersistenceDispatcher PersistenceDispatcher { get; }

    /// <summary>
    ///     Dispatcher instance
    /// </summary>
    /// <value></value>
    public IDispatcher Dispatcher { get; }

    /// <summary>
    ///     Scheduler instance
    /// </summary>
    /// <value></value>
    public IScheduler Scheduler { get; }

    /// <summary>
    ///     Sets game state as opened
    /// </summary>
    public void Open()
    {
        State = GameState.Opened;
        OnOpened?.Invoke();
    }

    /// <summary>
    ///     Sets game state as closed
    ///     No one can logIn on game expect GM
    /// </summary>
    public void Close()
    {
        State = GameState.Closed;
    }

    public event OpenServer OnOpened;
}