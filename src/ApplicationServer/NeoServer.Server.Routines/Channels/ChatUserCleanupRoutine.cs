using NeoServer.Game.Common.Contracts.Chats;

namespace NeoServer.Server.Routines.Channels;

public class ChatUserCleanupRoutine
{
    public static void Execute(IChatChannel channel)
    {
        foreach (var user in channel.Users)
        {
            if (!user.Removed || user.IsMuted) continue;
        
            channel.RemoveUser(user.Player);
        }
    }
}