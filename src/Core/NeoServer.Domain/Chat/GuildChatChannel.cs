using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Guild;

namespace NeoServer.Domain.Chat;

public class GuildChatChannel(ushort id, string name, Guild.Guild guild) : ChatChannel(id, name)
{
    private Guild.Guild Guild { get; } = guild;

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