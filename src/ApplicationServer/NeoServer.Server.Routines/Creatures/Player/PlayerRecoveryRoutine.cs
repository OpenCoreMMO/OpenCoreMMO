using NeoServer.Game.Common.Contracts.Creatures;

namespace NeoServer.Server.Routines.Creatures.Player;

public static class PlayerRecoveryRoutine
{
    public static void Execute(IPlayer player)
    {
        if (player.IsDead) return;
        if (!player.Recovering) return;

        player.Recover();
    }
}