using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Common.Texts;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Networking.Packets.Outgoing.Effect;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Events.Player;

public class PlayerExhaustedEventHandler
{
    private readonly IGameServer game;

    public PlayerExhaustedEventHandler(IGameServer game)
    {
        this.game = game;
    }

    public void Execute(IPlayer player)
    {
        if (!game.CreatureManager.GetPlayerConnection(player.CreatureId, out var connection)) return;

        connection.OutgoingPackets.Enqueue(new MagicEffectPacket(player.Location, EffectT.Puff));
        connection.OutgoingPackets.Enqueue(new TextMessagePacket(TextConstants.YOU_ARE_EXHAUSTED,
            TextMessageOutgoingType.Small));

        connection.Send();
    }
}