using System;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Creatures.Conditions.Enums;
using NeoServer.Server.Common.Contracts;

namespace NeoServer.Server.Routines.Creatures.Player;

public class PlayerStatusRoutine(GameConfiguration gameConfiguration, IGameServer gameServer) : IRoutine
{
    public void Execute(IPlayer player)
    {
        RemoveLogoutBlockIfExpired(player);
        RemoveProtectionZoneBlockIfExpired(player);
    }

    private void RemoveLogoutBlockIfExpired(IPlayer player)
    {
        if (!player.HasCondition(ConditionType.LogoutBlock, out var logoutBlockCondition)) return;

        var passedTicks = DateTime.UtcNow.Ticks - logoutBlockCondition.StartedAt;
        var milliseconds = new TimeSpan(passedTicks).TotalMilliseconds;

        if (milliseconds >= gameConfiguration.LogoutBlockDuration)
        {
            // Check if there are hostile monsters nearby before removing logout block
            if (HasHostileMonstersNearby(player))
            {
                // Reset the logout block condition to extend its duration
                player.RestartCondition(logoutBlockCondition);
                return;
            }

            player.RemoveLogoutBlock();
        }
    }

    private bool HasHostileMonstersNearby(IPlayer player)
    {
        var spectators = gameServer.Map.GetSpectators(player.Location);

        foreach (var spectator in spectators)
            if (spectator is IMonster monster && monster.IsHostileTo(player))
                return true;

        return false;
    }

    private void RemoveProtectionZoneBlockIfExpired(IPlayer player)
    {
        if (!player.HasCondition(ConditionType.ProtectionZoneBlock, out var protectionZoneBlock)) return;

        var passedTicks = DateTime.UtcNow.Ticks - protectionZoneBlock.StartedAt;
        var milliseconds = new TimeSpan(passedTicks).TotalMilliseconds;

        if (milliseconds >= gameConfiguration.ProtectionZoneBlockDuration) player.RemoveProtectionZoneBlock();
    }
}