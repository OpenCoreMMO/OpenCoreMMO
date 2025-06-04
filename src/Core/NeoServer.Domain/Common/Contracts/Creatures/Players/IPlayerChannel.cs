using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Contracts.Chats;
using NeoServer.Domain.Common.Contracts.DataStores;

namespace NeoServer.Domain.Common.Contracts.Creatures.Players;

public delegate void PlayerJoinChannel(IPlayer player, ChatChannel channel);

public delegate void PlayerExitChannel(IPlayer player, ChatChannel channel);

public interface IPlayerChannel
{
    IEnumerable<ChatChannel> PersonalChannels { get; }
    IEnumerable<ChatChannel> PrivateChannels { get; }
    bool CanEnterOnChannel(ushort channelId, IChatChannelStore chatChannelStore);
    void AddPersonalChannel(ChatChannel channel);
    bool JoinChannel(ChatChannel channel);
    bool ExitChannel(ChatChannel channel);
    bool SendMessage(ChatChannel channel, string message);
    event PlayerJoinChannel OnJoinedChannel;
    event PlayerExitChannel OnExitedChannel;
}