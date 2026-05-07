using System;
using NeoServer.Data.Interfaces;
using NeoServer.Networking.Packets.Incoming.Chat;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Networking.Packets.Outgoing.Chat;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Handlers.Chat;

public class PlayerOpenPrivateChannelHandler : PacketHandler
{
    private readonly IGameServer _game;
    private readonly IPlayerRepository _playerRepository;

    public PlayerOpenPrivateChannelHandler(IGameServer game, IPlayerRepository playerRepository)
    {
        _game = game;
        _playerRepository = playerRepository;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        var channel = new OpenPrivateChannelPacket(message);
        if (!_game.CreatureManager.TryGetPlayer(connection.CreatureId, out var player)) return;

        if (string.IsNullOrWhiteSpace(channel.Receiver) ||
            _playerRepository.GetByName(channel.Receiver) is null)
        {
            connection.Send(new TextMessagePacket("A player with this name does not exist.",
                TextMessageOutgoingType.Small));
            return;
        }

        if (string.IsNullOrWhiteSpace(channel.Receiver) ||
            !_game.CreatureManager.TryGetPlayer(channel.Receiver, out var receiver))
        {
            connection.Send(new TextMessagePacket("A player with this name is not online.",
                TextMessageOutgoingType.Small));
            return;
        }

        if (channel.Receiver.Trim().Equals(player.Name.Trim(), StringComparison.InvariantCultureIgnoreCase))
        {
            connection.Send(new TextMessagePacket("You cannot set up a private message channel with yourself.",
                TextMessageOutgoingType.Small));
            return;
        }

        if (receiver is null) return;

        connection.OutgoingPackets.Enqueue(new PlayerOpenPrivateChannelPacket(receiver.Name));
        connection.Send();
    }
}