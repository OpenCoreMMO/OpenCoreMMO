using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Networking.Packets.Outgoing.Effect;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Player;

public class PlayerOperationFailedEventHandler(IGameServer game)
{
    public void Execute(uint playerId, string message, EffectT effect = EffectT.None)
    {
        if (!game.CreatureManager.GetPlayerConnection(playerId, out var connection)) return;

        connection.OutgoingPackets.Enqueue(new TextMessagePacket(message,
            TextMessageOutgoingType.MESSAGE_STATUS_DEFAULT));

        if (effect != EffectT.None && game.CreatureManager.TryGetPlayer(playerId, out var player))
            connection.OutgoingPackets.Enqueue(new MagicEffectPacket(player.Location, effect));

        connection.Send();
    }

    public void Execute(uint playerId, InvalidOperation invalidOperation, EffectT effect = EffectT.None)
    {
        if (!game.CreatureManager.GetPlayerConnection(playerId, out var connection)) return;

        connection.OutgoingPackets.Enqueue(new TextMessagePacket(TextMessageOutgoingParser.Parse(invalidOperation),
            TextMessageOutgoingType.MESSAGE_STATUS_DEFAULT));

        if (effect != EffectT.None && game.CreatureManager.TryGetPlayer(playerId, out var player))
            connection.OutgoingPackets.Enqueue(new MagicEffectPacket(player.Location, effect));

        connection.Send();
    }
}