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
        // Administrators can access any guild channel
        if (player?.Group?.Access == true)
        {
            if (HasUser(player)) 
            {
                return false;
            }
            
            if (users.TryGetValue(player.Id, out var adminUser))
            {
                if (adminUser.Removed)
                {
                    adminUser.MarkAsAdded();
                    return true;
                }
                return false;
            }

            return users.TryAdd(player.Id, new UserChat { Player = player });
        }
        
        // Regular guild member validation
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
        // Administrators get special red color in guild channels
        if (player?.Group?.Access == true)
        {
            return SpeechType.ChannelRed1; // Admin gets leader color
        }
        
        var rank = player.GuildRank;
        
        // If rank is null, use default color
        if (rank == null)
        {
            return SpeechType.ChannelYellow; // Default to member color
        }
        
        return rank.Level switch
        {
            3 => SpeechType.ChannelRed1,    // Leader - red color
            2 => SpeechType.ChannelOrange,  // Vice-Leader - orange color  
            1 => SpeechType.ChannelYellow,  // Member - yellow color
            _ => SpeechType.ChannelYellow   // Default - yellow color
        };
    }
}