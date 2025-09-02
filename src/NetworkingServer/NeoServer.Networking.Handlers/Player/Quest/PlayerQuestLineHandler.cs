using NeoServer.Networking.Packets.Incoming;
using NeoServer.Server.Commands.Player.Quest;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Tasks;

namespace NeoServer.Networking.Handlers.Player.Quest;

public class PlayerQuestLineHandler(IGameServer game, PlayerOpenQuestLineCommand playerOpenQuestLineCommand)
    : PacketHandler
{
    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        var packet = new QuestLinePacket(message);

        if (!game.CreatureManager.TryGetPlayer(connection.CreatureId, out var player)) return;

        game.Dispatcher.AddEvent(new Event(2000,
            () => playerOpenQuestLineCommand.Execute(player, packet.QuestId)));
    }
}