using System.Linq;
using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Contracts.Chats;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Networking.Packets.Incoming.Chat;
using NeoServer.Server.Common.Contracts;
using NeoServer.Server.Common.Contracts.Network;
using NeoServer.Server.Tasks;

namespace NeoServer.Networking.Handlers.Chat;

public class PlayerCloseChannelHandler : PacketHandler
{
    private readonly IChatChannelStore _chatChannelStore;
    private readonly IGameServer _game;

    public PlayerCloseChannelHandler(IGameServer game, IChatChannelStore chatChannelStore)
    {
        _game = game;
        _chatChannelStore = chatChannelStore;
    }

    public override void HandleMessage(IReadOnlyNetworkMessage message, IConnection connection)
    {
        var channelPacket = new OpenChannelPacket(message);
        if (!_game.CreatureManager.TryGetPlayer(connection.CreatureId, out var player)) return;

        ChatChannel channel = null;

        if (_chatChannelStore.Get(channelPacket.ChannelId) is { } publicChannel)
            channel = publicChannel;

        if (player.Channels.PersonalChannels?.FirstOrDefault(x => x.Id == channelPacket.ChannelId) is
            { } personalChannel) channel = personalChannel;
        if (player.Channels.PrivateChannels?.FirstOrDefault(x => x.Id == channelPacket.ChannelId) is
            { } privateChannel) channel = privateChannel;

        if (channel is null) return;

        _game.Dispatcher.AddEvent(new Event(() => player.Channels.ExitChannel(channel)));
    }
}