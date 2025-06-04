using NeoServer.Domain.Common.Chats;
using NeoServer.Domain.Common.Contracts.Chats;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures.Guilds;
using NeoServer.Domain.Creatures.Guild;

namespace NeoServer.Domain.Chat;

public class GuildChatChannel : ChatChannel
{
    public GuildChatChannel(ushort id, string name, Guild guild) : base(id, name)
    {
        Guild = guild;
    }

    private Guild Guild { get; }

    public override bool Opened
    {
        get => true;
        init => base.Opened = value;
    }

    public override bool AddUser(IPlayer player)
    {
        if (player.Guild is null) return false;
        if (Guild is null) return false;

        if (!Guild.HasMember(player)) return false;

        return base.AddUser(player);
    }

    public override SpeechType GetTextColor(IPlayer player)
    {
        if (Guild.GetMemberLevel(player) is not { } guildMember) return SpeechType.ChannelYellowText;

        return guildMember.Level switch
        {
            GuildRank.Leader => SpeechType.ChannelOrangeText,
            _ => SpeechType.ChannelYellowText
        };
    }
}