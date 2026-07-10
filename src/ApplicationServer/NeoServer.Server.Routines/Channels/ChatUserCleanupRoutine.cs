using System.Collections.Generic;
using NeoServer.Domain.Chat;
using NeoServer.Domain.Common.Contracts.Creatures;

namespace NeoServer.Server.Routines.Channels;

public class ChatUserCleanupRoutine
{
    public static void Execute(ChatChannel channel)
    {
        var usersToRemove = new List<IPlayer>();

        foreach (var user in channel.Users)
        {
            if (user is null) continue;
            if (!user.Removed || user.IsMuted) continue;
            if (user.Player is null) continue;

            usersToRemove.Add(user.Player);
        }

        foreach (var player in usersToRemove)
            channel.RemoveUser(player);
    }
}

