using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Game.Common.Contracts.Items;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Player;

public class NotificationSentEventHandler
{
    private readonly IGameServer game;

    public NotificationSentEventHandler(IGameServer game)
    {
        this.game = game;
    }

    public void Execute(IThing thing, string message, TextMessageOutgoingType type)
    {
        if (thing is not IPlayer player) return;
        if (!game.CreatureManager.GetPlayerConnection(player.CreatureId, out var connection)) return;

        connection.OutgoingPackets.Enqueue(new TextMessagePacket(message,
            type));
        connection.Send();
    }
}