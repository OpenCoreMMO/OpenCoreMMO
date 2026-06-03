using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;

namespace NeoServer.Domain.Creatures.Services;

public class PlayerChannelService(IChatChannelStore chatChannelStore)
{
    public void ExitChannels(IPlayer player)
    {
        foreach (var channel in chatChannelStore.All.Where(x => x.HasUser(player)))
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

    public void JoinChannels(IPlayer player)
    {
        var channels = chatChannelStore.All.Where(x => x.Opened);

        channels = player.Channels.PersonalChannels is null
            ? channels
            : channels.Concat(player.Channels.PersonalChannels?.Where(x => x.Opened) ?? []);

        channels = player.Channels.PrivateChannels is not { } privateChannels
            ? channels
            : channels.Concat(privateChannels.Where(x => x.Opened));

        foreach (var channel in channels)
        {
            if (!channel.HasUser(player))
                player.Channels.JoinChannel(channel);
        }
    }
}