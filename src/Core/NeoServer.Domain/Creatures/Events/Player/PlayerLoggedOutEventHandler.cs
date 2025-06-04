using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;

namespace NeoServer.Domain.Creatures.Events.Player;

public class PlayerLoggedOutEventHandler : IGameEventHandler
{
    private readonly IChatChannelStore _chatChannelStore;

    public PlayerLoggedOutEventHandler(IChatChannelStore chatChannelStore)
    {
        _chatChannelStore = chatChannelStore;
    }

    public void Execute(IPlayer player)
    {
        ExitChannels(player);
    }

    private void ExitChannels(IPlayer player)
    {
        foreach (var channel in _chatChannelStore.All.Where(x => x.HasUser(player)))
            player.Channels.ExitChannel(channel);

        if (player.Channels.PersonalChannels is not null)
            foreach (var channel in player.Channels.PersonalChannels)
                player.Channels.ExitChannel(channel);

        if (player.Channels.PrivateChannels is not { } privateChatChannels) return;
        {
            foreach (var channel in privateChatChannels)
                player.Channels.ExitChannel(channel);
        }
    }
}