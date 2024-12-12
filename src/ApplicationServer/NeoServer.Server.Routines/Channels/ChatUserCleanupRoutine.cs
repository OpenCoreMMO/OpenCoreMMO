using System.Linq;
using NeoServer.Game.Common.Contracts.Chats;

namespace NeoServer.Server.Routines.Channels;

public class ChatUserCleanupRoutine
{
    public static void Execute(IChatChannel channel)
    {
        var removedUsers = channel.Users.Where(x => x.Removed && !x.IsMuted);

        foreach (var user in removedUsers) channel.RemoveUser(user.Player);
    }
}