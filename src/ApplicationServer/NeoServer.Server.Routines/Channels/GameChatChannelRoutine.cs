using NeoServer.Game.Common.Contracts.DataStores;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Tasks;

namespace NeoServer.Server.Routines.Channels;

public class GameChatChannelRoutine
{
    private const ushort EVENT_CHECK_ITEM_INTERVAL = 10000;
    private readonly IChatChannelStore _chatChannelStore;
    private readonly IGameServer _game;

    public GameChatChannelRoutine(IGameServer game, IChatChannelStore chatChannelStore)
    {
        _game = game;
        _chatChannelStore = chatChannelStore;
    }

    public void StartChecking()
    {
        _game.Scheduler.AddEvent(new SchedulerEvent(EVENT_CHECK_ITEM_INTERVAL, StartChecking));

        foreach (var channel in _chatChannelStore.All) ChatUserCleanupRoutine.Execute(channel);
    }
}