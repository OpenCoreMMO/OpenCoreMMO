using System;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Server.Routines.Creatures.Player;

public static class PlayerSkullRoutine
{
    public static void Execute(IPlayer player)
    {
        if (player.SkullEndsAt is null) return;
        if (player.SkullEndsAt > DateTime.Now) return;
        
        player.RemoveSkull();
    }
}