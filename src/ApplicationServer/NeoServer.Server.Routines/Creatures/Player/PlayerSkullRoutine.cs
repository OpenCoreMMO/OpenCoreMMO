using System;
using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Server.Routines.Creatures.Player;

public static class PlayerSkullRoutine
{
    public static void Execute(IPlayer player)
    {
        RemoveSkull(player);
        RemoveYellowSkull(player);
    }

    private static void RemoveYellowSkull(IPlayer player)
    {
        if (player.PlayerSkull.YellowSkullEndsAt > DateTime.Now) return;

        player.PlayerSkull.RemoveYellowSkull();
    }

    private static void RemoveSkull(IPlayer player)
    {
        if (player.SkullEndsAt is null) return;
        if (player.SkullEndsAt > DateTime.Now) return;

        player.RemoveSkull();
    }
}