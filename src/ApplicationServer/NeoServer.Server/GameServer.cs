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

    private const int EVENT_LIGHTINTERVAL_MS = 10000;
    private const int DAY_LENGTH_SECONDS = 3600;
    private const int LIGHT_DAY_LENGTH = 1440;
    private const int LIGHT_LEVEL_DAY = 250;
    private const int LIGHT_LEVEL_NIGHT = 40;
    private const int SUNSET = 1050;
    private const int SUNRISE = 360;

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public static IGameServer Instance { get; private set; }

    /// <summary>
    ///     Game's light level
    /// </summary>
    public byte LightLevel => LIGHT_LEVEL_DAY;

    /// <summary>
    ///     Indicates Game's light color
    /// </summary>
    public byte LightColor => 215;

    /// <summary>
    ///     Game's light hour
    /// </summary>
    public int LightHour => SUNRISE + (SUNSET - SUNRISE) / 2;

    /// <summary>
    ///     Game's light hour
    /// </summary>
    public int LightHourDelta => (LIGHT_DAY_LENGTH * (EVENT_LIGHTINTERVAL_MS / 1000)) / DAY_LENGTH_SECONDS;

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