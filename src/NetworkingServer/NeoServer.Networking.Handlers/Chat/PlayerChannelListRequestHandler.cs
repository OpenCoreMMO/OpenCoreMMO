using System.Linq;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Networking.Packets.Outgoing.Chat;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;

namespace NeoServer.Networking.Handlers.Chat;

public class PlayerChannelListRequestHandler : PacketHandler
{
    private readonly IChatChannelStore _chatChannelStore;
    private readonly IGameServer _game;

    public PlayerChannelListRequestHandler(IGameServer game, IChatChannelStore chatChannelStore)
    {
        _game = game;
        _chatChannelStore = chatChannelStore;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        if (!_game.CreatureManager.TryGetPlayer(connection.CreatureId, out var player)) return;

        var channels = _chatChannelStore.All.Where(x => x.PlayerCanJoin(player));
        channels = player.Channels.PersonalChannels is null
            ? channels
            : channels.Concat(player.Channels.PersonalChannels);
        channels = player.Channels.PrivateChannels is not { } privateChannels
            ? channels
            : channels.Concat(privateChannels);

        // Filter out channels with null names to prevent crashes
        var validChannels = channels.Where(x => !string.IsNullOrEmpty(x?.Name)).ToArray();

        connection.OutgoingPackets.Enqueue(new PlayerChannelListPacket(validChannels));
        connection.Send();
    }
}