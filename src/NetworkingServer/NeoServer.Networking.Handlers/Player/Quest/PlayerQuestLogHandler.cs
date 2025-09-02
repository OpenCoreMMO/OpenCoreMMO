using NeoServer.Server.Commands.Player.Quest;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Tasks;

namespace NeoServer.Networking.Handlers.Player.Quest;

public class PlayerQuestLogHandler(IGameServer game, PlayerOpenQuestLogCommand playerOpenQuestLogCommand) : PacketHandler
{
    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        if (!game.CreatureManager.TryGetPlayer(connection.CreatureId, out var player)) return;

        game.Dispatcher.AddEvent(new Event(2000,
            () => playerOpenQuestLogCommand.Execute(player)));
    }
}