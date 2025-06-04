using System;
using NeoServer.Domain.Common;
using NeoServer.Domain.Common.Contracts.Creatures;
using NeoServer.Domain.Common.Creatures;
using NeoServer.Domain.Creatures.Condition;

namespace NeoServer.Server.Routines.Creatures.Player;

public class PlayerStatusRoutine(GameConfiguration gameConfiguration) : IRoutine
{
    public void Execute(IPlayer player)
    {
        RemoveLogoutBlockIfExpired(player);
        RemoveProtectionZoneBlockIfExpired(player);
    }

    private void RemoveLogoutBlockIfExpired(IPlayer player)
    {
        if (!player.HasCondition(ConditionType.LogoutBlock, out var logoutBlockCondition)) return;

        var passedTicks = DateTime.Now.Ticks - logoutBlockCondition.StartedAt;
        var milliseconds = new TimeSpan(passedTicks).TotalMilliseconds;

        if (milliseconds >= gameConfiguration.LogoutBlockDuration) player.RemoveLogoutBlock();
    }

    private void RemoveProtectionZoneBlockIfExpired(IPlayer player)
    {
        if (!player.HasCondition(ConditionType.ProtectionZoneBlock, out var protectionZoneBlock)) return;

        var passedTicks = DateTime.Now.Ticks - protectionZoneBlock.StartedAt;
        var milliseconds = new TimeSpan(passedTicks).TotalMilliseconds;

        if (milliseconds >= gameConfiguration.ProtectionZoneBlockDuration) player.RemoveProtectionZoneBlock();
    }
}