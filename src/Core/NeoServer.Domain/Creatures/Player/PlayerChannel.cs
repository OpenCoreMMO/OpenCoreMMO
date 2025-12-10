using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.Creatures.Players;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Helpers;
using NeoServer.Domain.Common.Services;

namespace NeoServer.Domain.Creatures.Player;

public delegate void PlayerJoinChannel(IPlayer player, ChatChannel channel);

public delegate void PlayerExitChannel(IPlayer player, ChatChannel channel);
public class PlayerChannel(IPlayer owner)
{
    private IDictionary<ushort, ChatChannel> _personalChannels;

    private uint CreatureId => owner.CreatureId;

    public IEnumerable<ChatChannel> PersonalChannels => _personalChannels?.Values;

    public bool CanEnterOnChannel(ushort channelId, IChatChannelStore chatChannelStore)
    {
        var channel = chatChannelStore.Get(channelId);
        return channel?.PlayerCanJoin(owner) ?? false;
    }

    public IEnumerable<ChatChannel> PrivateChannels
    {
        get
        {
            if (owner.HasGuild && owner.Guild?.Channel is not null) yield return owner.Guild.Channel;
            if (owner.PlayerParty.Party?.Channel is not null) yield return owner.PlayerParty.Party.Channel;
        }
    }

    public void AddPersonalChannel(ChatChannel channel)
    {
        if (Guard.IsNull(channel)) return;

        _personalChannels ??= new Dictionary<ushort, ChatChannel>();
        _personalChannels.Add(channel.Id, channel);
    }

    public bool JoinChannel(ChatChannel channel)
    {
        if (channel is null) return false;

        if (channel.HasUser(owner))
        {
            OperationFailService.Send(CreatureId, "You've already joined this chat channel");
            return false;
        }

        if (!channel.AddUser(owner))
        {
            OperationFailService.Send(CreatureId, "You cannot join this chat channel");
            return false;
        }

        OnJoinedChannel?.Invoke(owner, channel);
        return true;
    }

    public bool ExitChannel(ChatChannel channel)
    {
        if (channel is null) return false;

        if (!channel.HasUser(owner)) return false;
        if (!channel.RemoveUser(owner))
        {
            OperationFailService.Send(CreatureId, "You cannot exit this chat channel");
            return false;
        }

        OnExitedChannel?.Invoke(owner, channel);
        return true;
    }

    public bool SendMessage(ChatChannel channel, string message)
    {
        if (!channel.WriteMessage(owner, message, out var cancelMessage))
        {
            OperationFailService.Send(CreatureId, cancelMessage);
            return false;
        }

        return true;
    }

    public event PlayerJoinChannel OnJoinedChannel;
    public event PlayerExitChannel OnExitedChannel;
}