using NeoServer.Domain.Common.Contracts.Creatures;

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
        if (player.Guild is null) 
        {
            return false;
        }
        
        if (Guild is null) 
        {
            return false;
        }

        if (!Guild.HasMember(player)) 
        {
            return false;
        }

        // Skip the base PlayerCanJoin validation and go directly to the core AddUser logic
        if (HasUser(player)) 
        {
            return false;
        }
        
        if (users.TryGetValue(player.Id, out var user))
        {
            if (user.Removed)
            {
                user.MarkAsAdded();
                return true;
            }
            return false;
        }

        var success = users.TryAdd(player.Id, new UserChat { Player = player });
        return success;
    }

    public override SpeechType GetTextColor(IPlayer player)
    {
        var rank = player.GuildRank;
        
        // If rank is null, use default color
        if (rank == null)
        {
            return SpeechType.ChannelYellowText; // Default to member color
        }
        
        return rank.Level switch
        {
            3 => SpeechType.ChannelRed1Text,    // Leader - red color
            2 => SpeechType.ChannelOrangeText,  // Vice-Leader - orange color  
            1 => SpeechType.ChannelYellowText,  // Member - yellow color
            _ => SpeechType.ChannelYellowText   // Default - yellow color
        };
    }
}