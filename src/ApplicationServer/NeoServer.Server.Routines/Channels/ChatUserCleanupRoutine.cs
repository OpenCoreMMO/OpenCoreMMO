using System.Linq;
using NeoServer.Domain.Chat;

namespace NeoServer.Server.Routines.Channels;

public class ChatUserCleanupRoutine
{
    public static void Execute(ChatChannel channel)
    {
        foreach (var user in channel.Users.ToList())
        {
            if (!user.Removed || user.IsMuted) continue;

            channel.RemoveUser(user.Player);
        }
    }
}