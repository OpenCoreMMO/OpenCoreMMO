using System;
using NeoServer.Game.Common.Contracts.Creatures;
using NeoServer.Networking.Packets.Outgoing;
using NeoServer.Server.Commands.Player;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Routines.Creatures.Player;

public static class PlayerPingRoutine
{
    private const int PING_INTERVAL = 5000;
    private const int CONNECTION_LOST_INTERVAL = 60000;

    public static void Execute(IPlayer player, PlayerLogOutCommand playerLogOutCommand, IGameServer game)
    {
        if (player.IsDead) return;

        var now = DateTime.Now.Ticks;

        if (!game.CreatureManager.GetPlayerConnection(player.CreatureId, out var connection)) return;

        var remainingTime = TimeSpan.FromTicks(now - connection.LastPingRequest).TotalMilliseconds;

        if (remainingTime >= PING_INTERVAL)
        {
            connection.LastPingRequest = now;

            connection.Send(new PingPacket());
        }

        var noPongTime = TimeSpan.FromTicks(now - connection.LastPingResponse).TotalMilliseconds;

        if (noPongTime >= CONNECTION_LOST_INTERVAL)
            playerLogOutCommand.Execute(player, true);
    }
}