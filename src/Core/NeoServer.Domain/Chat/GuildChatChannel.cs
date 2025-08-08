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
        Console.WriteLine($"[GuildChannel] AddUser called for player: {player?.Name ?? "null"}");
        
        if (player.Guild is null) 
        {
            Console.WriteLine($"[GuildChannel] Player {player.Name} has no guild (player.Guild is null)");
            return false;
        }
        
        if (Guild is null) 
        {
            Console.WriteLine($"[GuildChannel] Channel has no guild (Guild is null)");
            return false;
        }

        Console.WriteLine($"[GuildChannel] Player guild: {player.Guild?.Name} (ID: {player.Guild?.Id}), Channel guild: {Guild?.Name} (ID: {Guild?.Id})");
        Console.WriteLine($"[GuildChannel] Player.GuildId: {player.GuildId}");

        if (!Guild.HasMember(player)) 
        {
            Console.WriteLine($"[GuildChannel] Guild.HasMember returned false for player {player.Name}");
            return false;
        }

        Console.WriteLine($"[GuildChannel] Guild validation passed, checking if user already exists...");

        // Skip the base PlayerCanJoin validation and go directly to the core AddUser logic
        if (HasUser(player)) 
        {
            Console.WriteLine($"[GuildChannel] Player {player.Name} already in channel");
            return false;
        }
        
        if (users.TryGetValue(player.Id, out var user))
        {
            if (user.Removed)
            {
                Console.WriteLine($"[GuildChannel] Player {player.Name} was removed, marking as added");
                user.MarkAsAdded();
                return true;
            }
            Console.WriteLine($"[GuildChannel] Player {player.Name} already exists in users collection");
            return false;
        }

        var success = users.TryAdd(player.Id, new UserChat { Player = player });
        Console.WriteLine($"[GuildChannel] TryAdd result for player {player.Name}: {success}");
        return success;
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