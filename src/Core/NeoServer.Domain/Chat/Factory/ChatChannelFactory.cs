using NeoServer.Domain.Chat.Rules;
using NeoServer.Domain.Common.Contracts;
using NeoServer.Domain.Common.Contracts.Chats;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Contracts.DataStores;
using NeoServer.Domain.Common.Helpers;

namespace NeoServer.Domain.Chat.Factory;

public class ChatChannelFactory
{
    private readonly IEnumerable<IChatChannelEventSubscriber> _channelEventSubscribers;
    private readonly IChatChannelStore _chatChannelStore;
    private readonly IGuildStore _guildStore;

    public ChatChannelFactory(IEnumerable<IChatChannelEventSubscriber> channelEventSubscribers,
        IChatChannelStore chatChannelStore, IGuildStore guildStore)
    {
        _channelEventSubscribers = channelEventSubscribers;
        _chatChannelStore = chatChannelStore;
        _guildStore = guildStore;
    }

    public ChatChannel Create(Type type, string name, IPlayer player = null)
    {
        if (!typeof(ChatChannel).IsAssignableFrom(type)) return default;

        var id = typeof(PersonalChatChannel).IsAssignableTo(type) && player is not null
            ? GeneratePlayerUniqueId(player)
            : GenerateUniqueId();

        var channel = (ChatChannel)Activator.CreateInstance(type, id, name);

        SubscribeEvents(channel);

        return channel;
    }

    public ChatChannel CreateGuildChannel(string name, ushort guildId)
    {
        var id = GenerateUniqueId();
        var guid = _guildStore.Get(guildId);
        var channel = new GuildChatChannel(id, name, guid);
        SubscribeEvents(channel);
        return channel;
    }

    public ChatChannel CreateGuildChannel(string name, Guild.Guild guild)
    {
        var id = GenerateUniqueId();
        var channel = new GuildChatChannel(id, name, guild);
        SubscribeEvents(channel);
        return channel;
    }

    public ChatChannel CreatePartyChannel(string name = "Party")
    {
        var id = GenerateUniqueId();

        var channel = new ChatChannel(id, name);

        SubscribeEvents(channel);
        return channel;
    }

    public ChatChannel Create(
        ushort id,
        string name,
        string description,
        bool opened,
        SpeechType chatColor,
        Dictionary<byte, SpeechType> chatColorByVocation,
        ChannelRule joinRule,
        ChannelRule writeRule,
        MuteRule muteRule)
    {
        var channel = new ChatChannel(id, name)
        {
            Description = description,
            ChatColor = chatColor == SpeechType.None ? SpeechType.ChannelYellow : chatColor,
            ChatColorByVocation = chatColorByVocation ?? default,
            JoinRule = joinRule,
            WriteRule = writeRule,
            MuteRule = muteRule.None ? MuteRule.Default : muteRule,
            Opened = opened
        };

        SubscribeEvents(channel);

        return channel;
    }

    private ushort GenerateUniqueId()
    {
        ushort id;
        do
        {
            id = RandomIdGenerator.Generate(ushort.MaxValue);
        } while (_chatChannelStore.Contains(id));

        return id;
    }

    private ushort GeneratePlayerUniqueId(IPlayer player)
    {
        ushort id;
        do
        {
            id = GenerateUniqueId();
        } while ((player.Channels.PersonalChannels?.Any(x => x.Id == id) ?? false) ||
                 (player.Channels.PrivateChannels?.Any(x => x.Id == id) ?? false));

        return id;
    }

    //todo: move this method to a base factory to be used in other factories
    private void SubscribeEvents(ChatChannel createdChannel)
    {
        foreach (var gameSubscriber in _channelEventSubscribers.Where(x =>
                     x.GetType().IsAssignableTo(typeof(IGameEventSubscriber)))) //register game events first
            gameSubscriber.Subscribe(createdChannel);

        foreach (var subscriber in _channelEventSubscribers.Where(x =>
                     !x.GetType().IsAssignableTo(typeof(IGameEventSubscriber)))) //than register server events
            subscriber.Subscribe(createdChannel);
    }
}