using NeoServer.Domain.Chat.Rules;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Domain.Chat;

public delegate void AddMessage(ISociableCreature creature, ChatChannel channel, SpeechType speechType,
    string message);

public class ChatChannel
{
    protected IDictionary<uint, UserChat> users = new Dictionary<uint, UserChat>();

    public ChatChannel(ushort id, string name)
    {
        Id = id;
        Name = name;
    }

    public ChannelRule JoinRule { get; init; }
    public virtual ChannelRule WriteRule { get; init; }
    public MuteRule MuteRule { get; init; }
    public virtual SpeechType ChatColor { get; init; } = SpeechType.ChannelYellow;

    public Dictionary<byte, SpeechType> ChatColorByVocation { private get; init; }

    public ushort Id { get; }
    public virtual string Name { get; }
    public string Description { get; init; }
    public virtual bool Opened { get; init; }
    public virtual IEnumerable<UserChat> Users => users.Values;
    public event AddMessage OnMessageAdded;

    public virtual SpeechType GetTextColor(IPlayer player)
    {
        if (player is null) return ChatColor;
        
        // Staff colors based on group - applies to all channels
        if (player.Group != null)
        {
            switch (player.Group.Id)
            {
                case 6: // God (Administrator) - Red (same as other staff)
                    return SpeechType.ChannelRed1;
                case 5: // Community Manager - Red (same as GM to avoid protocol issues)
                    return SpeechType.ChannelRed1;
                case 4: // GameMaster - Red
                    return SpeechType.ChannelRed1;
                case 3: // Senior Tutor - Red (only in specific channels)
                    if (IsTutorChannel())
                        return SpeechType.ChannelRed1;
                    break;
                case 2: // Junior Tutor - Orange (only in specific channels)
                    if (IsTutorChannel())
                        return SpeechType.ChannelOrange;
                    break;
            }
        }
        
        // Default vocation-based color system
        if (ChatColorByVocation is not null &&
            ChatColorByVocation.TryGetValue(player.Vocation.VocationType, out var color)) return color;

        return ChatColor;
    }
    
    private bool IsTutorChannel()
    {
        // Channels where tutors have special colors: Help, English Chat, World Chat, Polish Chat, Portuguese Chat, Spanish Chat
        var tutorChannelNames = new[] { "Help", "English Chat", "World Chat", "Polish Chat", "Portuguese Chat", "Spanish Chat" };
        return tutorChannelNames.Contains(Name, StringComparer.OrdinalIgnoreCase);
    }

    public virtual bool HasUser(IPlayer player)
    {
        return users.TryGetValue(player.Id, out var user) && user.Removed == false;
    }

    public virtual bool AddUser(IPlayer player)
    {
        if (!PlayerCanJoin(player)) return false;

        if (users.TryGetValue(player.Id, out var user))
        {
            if (user.Removed)
            {
                user.MarkAsAdded();
                return true;
            }

            return false;
        }

        return users.TryAdd(player.Id, new UserChat { Player = player });
    }

    public virtual bool RemoveUser(IPlayer player)
    {
        if (users.TryGetValue(player.Id, out var user))
        {
            if (!user.IsMuted) users.Remove(player.Id);
            else user.MarkAsRemoved();
        }

        return true;
    }

    public bool PlayerCanJoin(IPlayer player)
    {
        return Validate(JoinRule, player);
    }

    public bool PlayerCanWrite(IPlayer player)
    {
        return player is null
            ? true
            : player is not null && users.ContainsKey(player.Id) && Validate(WriteRule, player);
    }

    public bool PlayerIsMuted(IPlayer player, out string cancelMessage)
    {
        cancelMessage = default;
        if (player is null) return false;

        if (users.TryGetValue(player.Id, out var user) && user.IsMuted)
        {
            cancelMessage = string.IsNullOrWhiteSpace(MuteRule.CancelMessage)
                ? $"You are muted for {user.RemainingMutedSeconds} seconds"
                : MuteRule.CancelMessage;
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Write a message on channel without identification, useful for server message
    /// </summary>
    /// <param name="message"></param>
    /// <param name="cancelMessage"></param>
    /// <returns></returns>
    public bool WriteMessage(string message, out string cancelMessage, SpeechType speechType = SpeechType.None)
    {
        cancelMessage = default;
        if (!(users?.Any() ?? false)) return false;

        var color = speechType == SpeechType.None ? ChatColor : speechType;

        OnMessageAdded?.Invoke(null, this, color, message);
        return true;
    }

    public bool WriteMessage(ISociableCreature creature, string message, out string cancelMessage,
        SpeechType speechType = SpeechType.None)
    {
        cancelMessage = default;

        var player = creature is IPlayer ? (IPlayer)creature : null;

        if (!PlayerCanWrite(player))
        {
            cancelMessage = "You cannot send message to this channel";
            return false;
        }

        if (PlayerIsMuted(player, out cancelMessage)) return false;

        if (users.TryGetValue(player?.Id ?? 0, out var user)) user.UpdateLastMessage(MuteRule);

        var color = speechType == SpeechType.None ? GetTextColor(player) : speechType;

        OnMessageAdded?.Invoke(creature, this, color, message);
        return true;
    }

    public bool Validate(ChannelRule rule, IPlayer player)
    {
        // Administrators bypass all channel restrictions
        if (player?.Group?.Access == true)
        {
            return true;
        }
        
        if (rule.None) return true;
        if (rule.AllowedVocations?.Length > 0 &&
            !rule.AllowedVocations.Contains(player.Vocation.VocationType)) return false;

        if (rule.MinMaxAllowedLevel.Item1 > 0 && player.Level <= rule.MinMaxAllowedLevel.Item1) return false;
        if (rule.MinMaxAllowedLevel.Item2 > 0 && player.Level > rule.MinMaxAllowedLevel.Item2) return false;
        return true;
    }
}